#!/usr/bin/env python3
"""Targeted Critical/High dependency remediation for samples/TeamsJS.

The script reads open Dependabot alerts, groups them by manifest, applies only the
minimum patched versions reported by GitHub, validates the affected sample, and
opens one PR per successfully validated manifest. Failed/conflicting manifests are
rolled back and recorded in a report.
"""
from __future__ import annotations

import argparse
import hashlib
import json
import os
import re
import shutil
import subprocess
import sys
import tempfile
import urllib.error
import urllib.parse
import urllib.request
import xml.etree.ElementTree as ET
from dataclasses import dataclass, asdict
from pathlib import Path
from typing import Iterable

ROOT_PREFIX = "samples/TeamsJS/"
SUPPORTED = {"npm", "pip", "nuget", "maven"}
SEVERITIES = {"critical", "high"}


@dataclass(frozen=True)
class Alert:
    number: int
    ecosystem: str
    package: str
    manifest: str
    severity: str
    fixed: str
    advisory: str


@dataclass
class Result:
    manifest: str
    ecosystem: str
    status: str
    packages: list[str]
    branch: str = ""
    pr_url: str = ""
    detail: str = ""


def run(cmd: list[str], cwd: Path | None = None, check: bool = True,
        env: dict[str, str] | None = None) -> subprocess.CompletedProcess[str]:
    print("+", " ".join(cmd), flush=True)
    cp = subprocess.run(cmd, cwd=cwd, text=True, stdout=subprocess.PIPE,
                        stderr=subprocess.STDOUT, env=env)
    if cp.stdout:
        print(cp.stdout[-12000:], flush=True)
    if check and cp.returncode:
        raise RuntimeError(f"command failed ({cp.returncode}): {' '.join(cmd)}\n{cp.stdout[-4000:]}")
    return cp


def api_json(url: str, token: str) -> tuple[object, dict[str, str]]:
    req = urllib.request.Request(url, headers={
        "Accept": "application/vnd.github+json",
        "Authorization": f"Bearer {token}",
        "X-GitHub-Api-Version": "2022-11-28",
        "User-Agent": "teamsjs-security-remediation",
    })
    try:
        with urllib.request.urlopen(req, timeout=60) as resp:
            return json.load(resp), {k.lower(): v for k, v in resp.headers.items()}
    except urllib.error.HTTPError as exc:
        body = exc.read().decode("utf-8", "replace")
        raise RuntimeError(f"GitHub API returned {exc.code} for {url}: {body[:1000]}") from exc


def fetch_alerts(repo: str, token: str) -> list[dict]:
    base = f"https://api.github.com/repos/{repo}/dependabot/alerts"
    page, rows = 1, []
    while True:
        url = f"{base}?state=open&per_page=100&page={page}"
        data, _ = api_json(url, token)
        if not isinstance(data, list):
            raise RuntimeError("Unexpected Dependabot alerts response")
        rows.extend(data)
        if len(data) < 100:
            return rows
        page += 1


def parse_alerts(rows: Iterable[dict], severities: set[str]) -> list[Alert]:
    selected: dict[tuple[str, str, str], Alert] = {}
    for row in rows:
        dep = row.get("dependency") or {}
        pkg = dep.get("package") or {}
        vuln = row.get("security_vulnerability") or {}
        adv = row.get("security_advisory") or {}
        manifest = str(dep.get("manifest_path") or "").replace("\\", "/").lstrip("/")
        severity = str(vuln.get("severity") or adv.get("severity") or "").lower()
        ecosystem = str(pkg.get("ecosystem") or "").lower()
        fixed_obj = vuln.get("first_patched_version") or {}
        fixed = str(fixed_obj.get("identifier") or "").strip()
        name = str(pkg.get("name") or "").strip()
        if (not manifest.startswith(ROOT_PREFIX) or severity not in severities
                or ecosystem not in SUPPORTED or not name or not fixed):
            continue
        alert = Alert(
            number=int(row.get("number") or 0), ecosystem=ecosystem,
            package=name, manifest=manifest, severity=severity, fixed=fixed,
            advisory=str(adv.get("ghsa_id") or ""),
        )
        key = (manifest.lower(), ecosystem, name.lower())
        old = selected.get(key)
        if not old or version_key(alert.fixed) > version_key(old.fixed):
            selected[key] = alert
    return sorted(selected.values(), key=lambda a: (a.manifest, a.package.lower()))


def version_key(value: str) -> tuple:
    parts = re.findall(r"\d+|[A-Za-z]+", value)
    return tuple((0, int(x)) if x.isdigit() else (1, x.lower()) for x in parts)


def group_alerts(alerts: Iterable[Alert]) -> list[tuple[str, str, list[Alert]]]:
    groups: dict[tuple[str, str], list[Alert]] = {}
    for alert in alerts:
        groups.setdefault((alert.manifest, alert.ecosystem), []).append(alert)
    return [(m, e, groups[(m, e)]) for m, e in sorted(groups)]


def safe_branch(ecosystem: str, manifest: str) -> str:
    stem = re.sub(r"[^a-z0-9]+", "-", manifest.lower()).strip("-")
    digest = hashlib.sha256(manifest.encode()).hexdigest()[:10]
    return f"copilot/teamsjs-security/{ecosystem}-{stem[:90]}-{digest}"


def semver_range(old: str, fixed: str) -> str:
    old = old.strip()
    m = re.match(r"^(\^|~|>=|>|=)?\s*\d", old)
    prefix = (m.group(1) or "") if m else ""
    return f"{prefix}{fixed}"


def resolve_npm_manifest(root: Path, manifest: str) -> Path:
    p = root / manifest
    if p.name == "package.json":
        return p
    candidate = p.parent / "package.json"
    if candidate.exists():
        return candidate
    raise RuntimeError(f"package.json not found next to {manifest}")


def patch_npm(root: Path, manifest: str, alerts: list[Alert]) -> Path:
    package_json = resolve_npm_manifest(root, manifest)
    data = json.loads(package_json.read_text(encoding="utf-8"))
    sections = ("dependencies", "devDependencies", "optionalDependencies", "peerDependencies")
    for alert in alerts:
        direct = False
        for section in sections:
            deps = data.get(section)
            if isinstance(deps, dict) and alert.package in deps:
                deps[alert.package] = semver_range(str(deps[alert.package]), alert.fixed)
                direct = True
        if not direct:
            overrides = data.setdefault("overrides", {})
            if not isinstance(overrides, dict):
                raise RuntimeError("package.json overrides is not an object")
            overrides[alert.package] = alert.fixed
    package_json.write_text(json.dumps(data, indent=2, ensure_ascii=False) + "\n", encoding="utf-8")
    return package_json


def validate_npm(package_json: Path) -> None:
    cwd = package_json.parent
    run(["npm", "install", "--package-lock-only", "--ignore-scripts", "--no-audit", "--no-fund"], cwd)
    run(["npm", "install", "--ignore-scripts", "--no-audit", "--no-fund"], cwd)
    audit = run(["npm", "audit", "--json", "--audit-level=high"], cwd, check=False)
    try:
        data = json.loads(audit.stdout or "{}")
        vulns = (data.get("metadata") or {}).get("vulnerabilities") or {}
        if int(vulns.get("high", 0)) + int(vulns.get("critical", 0)):
            raise RuntimeError(f"npm audit still reports High/Critical vulnerabilities: {vulns}")
    except json.JSONDecodeError:
        if audit.returncode:
            raise RuntimeError("npm audit failed and did not return JSON")
    scripts = json.loads(package_json.read_text(encoding="utf-8")).get("scripts") or {}
    if "build" in scripts:
        run(["npm", "run", "build"], cwd)
    elif "test" in scripts and not re.search(r"no test specified", str(scripts["test"]), re.I):
        env = os.environ.copy(); env["CI"] = "true"
        run(["npm", "test", "--", "--runInBand"], cwd, env=env)
    shutil.rmtree(cwd / "node_modules", ignore_errors=True)


def normalize_py_name(name: str) -> str:
    return re.sub(r"[-_.]+", "-", name).lower()


def patch_pip(root: Path, manifest: str, alerts: list[Alert]) -> Path:
    req = root / manifest
    if not req.exists():
        raise RuntimeError(f"requirements file not found: {manifest}")
    lines = req.read_text(encoding="utf-8").splitlines()
    for alert in alerts:
        found = False
        for i, line in enumerate(lines):
            raw = line.strip()
            if not raw or raw.startswith("#") or raw.startswith(("-r", "--")):
                continue
            name = re.split(r"[<>=!~\[;\s]", raw, 1)[0]
            if normalize_py_name(name) == normalize_py_name(alert.package):
                marker = (";" + raw.split(";", 1)[1]) if ";" in raw else ""
                lines[i] = f"{name}>={alert.fixed}{marker}"
                found = True
        if not found:
            lines.append(f"{alert.package}>={alert.fixed}  # security transitive pin")
    req.write_text("\n".join(lines) + "\n", encoding="utf-8")
    return req


def validate_pip(req: Path) -> None:
    cwd = req.parent
    venv = cwd / ".teamsjs-security-venv"
    shutil.rmtree(venv, ignore_errors=True)
    run([sys.executable, "-m", "venv", str(venv)], cwd)
    py = venv / ("Scripts/python.exe" if os.name == "nt" else "bin/python")
    run([str(py), "-m", "pip", "install", "--disable-pip-version-check", "pip-audit"], cwd)
    run([str(py), "-m", "pip", "install", "--disable-pip-version-check", "-r", req.name], cwd)
    run([str(py), "-m", "pip_audit", "-r", req.name, "--strict"], cwd)
    if (cwd / "pytest.ini").exists() or (cwd / "tests").is_dir():
        run([str(py), "-m", "pytest", "-q"], cwd)
    shutil.rmtree(venv, ignore_errors=True)


def find_project(path: Path, suffix: str) -> Path:
    if path.suffix.lower() == suffix:
        return path
    for parent in (path.parent, *path.parents):
        matches = sorted(parent.glob(f"*{suffix}"))
        if matches:
            return matches[0]
        if parent.name == "TeamsJS":
            break
    raise RuntimeError(f"No {suffix} project found near {path}")


def xml_ns(root: ET.Element) -> str:
    if root.tag.startswith("{"):
        return root.tag.split("}", 1)[0] + "}"
    return ""


def patch_nuget(root: Path, manifest: str, alerts: list[Alert]) -> Path:
    project = find_project(root / manifest, ".csproj")
    tree = ET.parse(project); doc = tree.getroot(); ns = xml_ns(doc)
    refs = doc.findall(f".//{ns}PackageReference")
    for alert in alerts:
        target = next((r for r in refs if (r.get("Include") or r.get("Update") or "").lower() == alert.package.lower()), None)
        if target is None:
            groups = doc.findall(f"{ns}ItemGroup")
            group = groups[-1] if groups else ET.SubElement(doc, f"{ns}ItemGroup")
            target = ET.SubElement(group, f"{ns}PackageReference", {"Include": alert.package, "Version": alert.fixed})
            refs.append(target)
        elif target.get("Version") is not None:
            target.set("Version", alert.fixed)
        else:
            v = target.find(f"{ns}Version")
            if v is None: v = ET.SubElement(target, f"{ns}Version")
            v.text = alert.fixed
    ET.indent(tree, space="  ")
    tree.write(project, encoding="utf-8", xml_declaration=True)
    return project


def validate_nuget(project: Path) -> None:
    run(["dotnet", "restore", str(project)], project.parent)
    run(["dotnet", "build", str(project), "--no-restore", "--configuration", "Release"], project.parent)
    cp = run(["dotnet", "list", str(project), "package", "--vulnerable", "--include-transitive"], project.parent)
    if re.search(r"\b(high|critical)\b", cp.stdout or "", re.I):
        raise RuntimeError("dotnet vulnerability scan still reports High/Critical packages")


def split_maven(name: str) -> tuple[str, str]:
    if ":" not in name:
        raise RuntimeError(f"Maven package is not groupId:artifactId: {name}")
    return tuple(name.split(":", 1))  # type: ignore[return-value]


def patch_maven(root: Path, manifest: str, alerts: list[Alert]) -> Path:
    pom = find_project(root / manifest, ".xml") if not manifest.endswith("pom.xml") else root / manifest
    tree = ET.parse(pom); doc = tree.getroot(); ns = xml_ns(doc)
    for alert in alerts:
        group_id, artifact_id = split_maven(alert.package)
        target = None
        for dep in doc.findall(f".//{ns}dependency"):
            g = dep.find(f"{ns}groupId"); a = dep.find(f"{ns}artifactId")
            if g is not None and a is not None and g.text == group_id and a.text == artifact_id:
                target = dep; break
        if target is None:
            dm = doc.find(f"{ns}dependencyManagement")
            if dm is None: dm = ET.SubElement(doc, f"{ns}dependencyManagement")
            deps = dm.find(f"{ns}dependencies")
            if deps is None: deps = ET.SubElement(dm, f"{ns}dependencies")
            target = ET.SubElement(deps, f"{ns}dependency")
            ET.SubElement(target, f"{ns}groupId").text = group_id
            ET.SubElement(target, f"{ns}artifactId").text = artifact_id
        version = target.find(f"{ns}version")
        if version is None: version = ET.SubElement(target, f"{ns}version")
        version.text = alert.fixed
    ET.indent(tree, space="  ")
    tree.write(pom, encoding="utf-8", xml_declaration=True)
    return pom


def validate_maven(pom: Path) -> None:
    mvn = "mvnw.cmd" if os.name == "nt" else "./mvnw"
    if not (pom.parent / mvn).exists(): mvn = "mvn"
    run([mvn, "-B", "-f", str(pom), "-DskipTests", "package"], pom.parent)


def apply_and_validate(root: Path, manifest: str, ecosystem: str, alerts: list[Alert]) -> None:
    if ecosystem == "npm":
        p = patch_npm(root, manifest, alerts); validate_npm(p)
    elif ecosystem == "pip":
        p = patch_pip(root, manifest, alerts); validate_pip(p)
    elif ecosystem == "nuget":
        p = patch_nuget(root, manifest, alerts); validate_nuget(p)
    elif ecosystem == "maven":
        p = patch_maven(root, manifest, alerts); validate_maven(p)
    else:
        raise RuntimeError(f"Unsupported ecosystem: {ecosystem}")


def stage_dependency_changes(root: Path, manifest: str, ecosystem: str) -> None:
    """Stage tracked changes plus only recognized newly-created dependency files."""
    run(["git", "add", "-u", "--", ROOT_PREFIX], root)
    parent = (root / manifest).parent
    names = {
        "npm": ("package.json", "package-lock.json", "npm-shrinkwrap.json"),
        "pip": (Path(manifest).name,),
        "nuget": ("packages.lock.json", "Directory.Packages.props"),
        "maven": ("pom.xml",),
    }[ecosystem]
    for name in names:
        candidate = parent / name
        if not candidate.exists():
            continue
        try:
            rel = candidate.relative_to(root).as_posix()
        except ValueError:
            continue
        if rel.startswith(ROOT_PREFIX):
            run(["git", "add", "--", rel], root)


def current_pr(branch: str) -> str:
    if not shutil.which("gh"):
        return ""
    cp = run(["gh", "pr", "list", "--head", branch, "--state", "open", "--json", "url", "--jq", ".[0].url"], check=False)
    return (cp.stdout or "").strip()


def title_for(ecosystem: str, manifest: str) -> str:
    sample = manifest[len(ROOT_PREFIX):].split("/", 1)[0]
    return f"Security: remediate {ecosystem} dependencies in TeamsJS {sample}"


def remediate(args: argparse.Namespace) -> list[Result]:
    root = Path(args.root).resolve()
    token = os.environ.get("DEPENDABOT_ALERTS_TOKEN") or os.environ.get("GITHUB_TOKEN") or ""
    if not args.alerts_file and not token:
        raise RuntimeError("Set DEPENDABOT_ALERTS_TOKEN (Dependabot alerts: read) or GITHUB_TOKEN")
    rows = json.loads(Path(args.alerts_file).read_text()) if args.alerts_file else fetch_alerts(args.repo, token)
    alerts = parse_alerts(rows, set(args.severity.split(",")))
    groups = group_alerts(alerts)
    print(f"Selected {len(alerts)} package fixes across {len(groups)} manifests")
    base_sha = run(["git", "rev-parse", args.base], root).stdout.strip()
    results: list[Result] = []
    attempted = 0
    for manifest, ecosystem, items in groups:
        branch = safe_branch(ecosystem, manifest)
        packages = [f"{a.package}>={a.fixed}" for a in items]
        existing = current_pr(branch)
        if existing:
            results.append(Result(manifest, ecosystem, "already-open", packages, branch, existing))
            continue
        if args.max_groups and attempted >= args.max_groups:
            results.append(Result(manifest, ecosystem, "deferred", packages, branch=branch,
                                  detail="Run limit reached; a later scheduled run will process this manifest."))
            continue
        attempted += 1
        run(["git", "checkout", "--force", "-B", branch, base_sha], root)
        run(["git", "clean", "-fdx", "--exclude=.github/scripts/teamsjs_security_remediation.py"], root)
        try:
            apply_and_validate(root, manifest, ecosystem, items)
            if run(["git", "diff", "--quiet"], root, check=False).returncode == 0:
                results.append(Result(manifest, ecosystem, "no-change", packages, branch=branch))
                continue
            stage_dependency_changes(root, manifest, ecosystem)
            run(["git", "commit", "-m", title_for(ecosystem, manifest)], root)
            if args.dry_run:
                results.append(Result(manifest, ecosystem, "validated-dry-run", packages, branch=branch))
                continue
            run(["git", "push", "--force-with-lease", "origin", f"HEAD:refs/heads/{branch}"], root)
            body = "Automated targeted remediation for open Critical/High Dependabot alerts.\n\n" + \
                   "Manifest: `" + manifest + "`\n\nPackages:\n" + \
                   "\n".join(f"- `{x}`" for x in packages) + \
                   "\n\nThe affected dependency restore/install, security scan, and available build validation completed successfully."
            cp = run(["gh", "pr", "create", "--base", args.base, "--head", branch,
                      "--title", title_for(ecosystem, manifest), "--body", body], root)
            results.append(Result(manifest, ecosystem, "pr-opened", packages, branch, (cp.stdout or "").strip()))
        except Exception as exc:
            run(["git", "reset", "--hard", base_sha], root, check=False)
            run(["git", "clean", "-fdx", "--exclude=.github/scripts/teamsjs_security_remediation.py"], root, check=False)
            results.append(Result(manifest, ecosystem, "blocked", packages, branch=branch, detail=str(exc)[-4000:]))
    run(["git", "checkout", "--force", args.base], root, check=False)
    return results


def write_report(results: list[Result], output: Path) -> None:
    summary = {
        "total": len(results),
        "pr_opened": sum(r.status == "pr-opened" for r in results),
        "validated_dry_run": sum(r.status == "validated-dry-run" for r in results),
        "already_open": sum(r.status == "already-open" for r in results),
        "blocked": sum(r.status == "blocked" for r in results),
        "results": [asdict(r) for r in results],
    }
    output.parent.mkdir(parents=True, exist_ok=True)
    output.write_text(json.dumps(summary, indent=2) + "\n", encoding="utf-8")
    print(json.dumps({k: v for k, v in summary.items() if k != "results"}, indent=2))
    step = os.environ.get("GITHUB_STEP_SUMMARY")
    if step:
        with open(step, "a", encoding="utf-8") as fh:
            fh.write("## TeamsJS security remediation\n\n")
            fh.write("| Manifest | Ecosystem | Status | PR |\n|---|---|---|---|\n")
            for r in results:
                fh.write(f"| `{r.manifest}` | {r.ecosystem} | {r.status} | {r.pr_url or ''} |\n")


def main() -> int:
    p = argparse.ArgumentParser()
    p.add_argument("--repo", required=True, help="owner/repository")
    p.add_argument("--root", default=".")
    p.add_argument("--base", default="main")
    p.add_argument("--severity", default="critical,high")
    p.add_argument("--alerts-file", help="offline Dependabot alerts JSON for testing")
    p.add_argument("--max-groups", type=int, default=0)
    p.add_argument("--dry-run", action="store_true")
    p.add_argument("--report", default="teamsjs-security-remediation-report.json")
    args = p.parse_args()
    try:
        results = remediate(args)
        write_report(results, Path(args.report))
        return 0 if not any(r.status == "blocked" for r in results) else 2
    except Exception as exc:
        print(f"ERROR: {exc}", file=sys.stderr)
        return 1


if __name__ == "__main__":
    raise SystemExit(main())
