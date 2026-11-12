// Opens the Teams app-install deep link exactly once, in a single tab of the chosen browser.
// Replaces the debugger's browser-launch (a throwaway, not-signed-in profile whose deep-link
// sign-in redirect produced a SECOND tab). Reads TEAMS_APP_ID from env/.env.local.
// Usage: node scripts/open-teams.js [edge|chrome]   (OPEN_TEAMS_DRYRUN=1 prints the command only)
const fs = require("fs");
const path = require("path");
const { spawn } = require("child_process");

const browser = (process.argv[2] || "default").toLowerCase();
const root = path.resolve(__dirname, "..");
const envPath = path.join(root, "env", ".env.local");

function readEnv(key) {
  if (!fs.existsSync(envPath)) return "";
  for (const line of fs.readFileSync(envPath, "utf8").split(/\r?\n/)) {
    const m = line.match(/^\s*([A-Za-z0-9_]+)\s*=\s*(.*?)\s*$/);
    if (m && m[1] === key) return m[2].trim();
  }
  return "";
}

const appId = readEnv("TEAMS_APP_ID");
if (!appId) {
  console.error("[open-teams] TEAMS_APP_ID not found in env/.env.local — run Provision first. Skipping.");
  process.exit(0); // never fail the debug session over this
}

const url = `https://teams.microsoft.com/l/app/${appId}?installAppPackage=true&webjoin=true`;

let command;
if (process.platform === "win32") {
  // 'start' is a cmd builtin; first quoted token is the window title. Quote the URL so the '&' in the
  // query string is not treated as a command separator.
  const target = browser === "edge" ? "msedge " : browser === "chrome" ? "chrome " : "";
  command = `start "" ${target}"${url}"`;
} else if (process.platform === "darwin") {
  const app = browser === "edge" ? "-a \"Microsoft Edge\" " : browser === "chrome" ? "-a \"Google Chrome\" " : "";
  command = `open ${app}"${url}"`;
} else {
  command = `xdg-open "${url}"`;
}

console.log(`[open-teams] ${browser}: ${command}`);
if (process.env.OPEN_TEAMS_DRYRUN === "1") process.exit(0);

const child = spawn(command, { stdio: "ignore", detached: true, shell: true });
child.on("error", (e) => console.error(`[open-teams] failed to open browser: ${e.message}`));
child.unref();
