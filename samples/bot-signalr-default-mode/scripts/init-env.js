// Seeds the gitignored TeamsFx env files from their committed .sample templates when they are
// missing, so a fresh clone can F5 straight into "Debug in Teams" with defaults prepopulated.
// An existing file is NEVER overwritten — safe to run on every launch.
const fs = require("fs");
const path = require("path");

const root = path.resolve(__dirname, "..");
const pairs = [
  { sample: "env/.env.local.sample", target: "env/.env.local" },
  { sample: "env/.env.local.user.sample", target: "env/.env.local.user" },
];

let created = 0;
for (const { sample, target } of pairs) {
  const targetPath = path.join(root, target);
  const samplePath = path.join(root, sample);

  if (fs.existsSync(targetPath)) {
    console.log(`[init-env] ${target} already exists — left untouched.`);
    continue;
  }
  if (!fs.existsSync(samplePath)) {
    console.warn(`[init-env] template ${sample} not found — skipped ${target}.`);
    continue;
  }

  fs.mkdirSync(path.dirname(targetPath), { recursive: true });
  fs.copyFileSync(samplePath, targetPath);
  created++;
  console.log(`[init-env] created ${target} from ${sample}.`);
}

console.log(`[init-env] done (${created} file(s) created).`);
