const fs = require("fs");
const path = require("path");
const { spawn } = require("child_process");

const [, , envFile, ...commandParts] = process.argv;

if (!envFile || commandParts.length === 0) {
  console.error("Usage: node ./scripts/run-with-env.js <env-file> <command> [args...]");
  process.exit(1);
}

const envFilePath = path.resolve(process.cwd(), envFile);
const parsedEnv = {};

if (fs.existsSync(envFilePath)) {
  const fileContents = fs.readFileSync(envFilePath, "utf8");

  fileContents.split(/\r?\n/).forEach((line) => {
    const trimmedLine = line.trim();

    if (!trimmedLine || trimmedLine.startsWith("#")) {
      return;
    }

    const separatorIndex = trimmedLine.indexOf("=");
    if (separatorIndex === -1) {
      return;
    }

    const key = trimmedLine.slice(0, separatorIndex).trim();
    const value = trimmedLine.slice(separatorIndex + 1).trim();

    if (key) {
      parsedEnv[key] = value;
    }
  });
}

const [command, ...args] = commandParts;
const child = spawn(command, args, {
  cwd: process.cwd(),
  env: { ...process.env, ...parsedEnv },
  shell: true,
  stdio: "inherit",
});

child.on("exit", (code, signal) => {
  if (signal) {
    process.kill(process.pid, signal);
    return;
  }

  process.exit(code ?? 0);
});
