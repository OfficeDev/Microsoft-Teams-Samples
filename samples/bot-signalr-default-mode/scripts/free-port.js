// Force-free a TCP port before launch so a relaunch never stops at VS Code's
// "Port <n> (PID: ...) is occupied. Terminate the corresponding process(es)" dialog.
//
// The Node debug/inspector port (9239) is held by the previous `node --inspect=9239` run when a
// debug session wasn't cleanly stopped (browser closed, process orphaned). The M365 Agents Toolkit
// "debug-check-prerequisites" portOccupancy check surfaces that as a modal. Running this first as a
// preLaunch step kills any lingering LISTENER on the port so the new run binds cleanly — no manual
// intervention. Mirrors killListenersOnPort() in src/index.ts (the in-process last resort).
//
// Usage: node scripts/free-port.js [port ...]   (defaults to 9239)
"use strict";

const { execSync } = require("child_process");

function killListenersOnPort(port) {
  const self = String(process.pid);
  try {
    if (process.platform === "win32") {
      const out = execSync("netstat -ano -p tcp", { encoding: "utf8", windowsHide: true });
      const pids = new Set();
      for (const line of out.split(/\r?\n/)) {
        const m = line.match(new RegExp(`[:.]${port}\\b.*\\bLISTENING\\b\\s+(\\d+)\\s*$`, "i"));
        if (m && m[1] !== self) {
          pids.add(m[1]);
        }
      }
      for (const pid of pids) {
        try {
          execSync(`taskkill /F /PID ${pid}`, { stdio: "ignore", windowsHide: true });
          console.log(`[free-port] force-freed :${port} (killed PID ${pid})`);
        } catch {
          /* already gone */
        }
      }
    } else {
      const out = execSync(`lsof -ti tcp:${port} -sTCP:LISTEN || true`, { encoding: "utf8" });
      for (const pid of out.split(/\s+/).filter(Boolean)) {
        if (pid !== self) {
          try {
            execSync(`kill -9 ${pid}`, { stdio: "ignore" });
            console.log(`[free-port] force-freed :${port} (killed PID ${pid})`);
          } catch {
            /* already gone */
          }
        }
      }
    }
  } catch (e) {
    console.warn(`[free-port] could not inspect/free :${port}: ${e && e.message}`);
  }
}

const ports = process.argv.slice(2).map(Number).filter((p) => Number.isInteger(p) && p > 0);
if (ports.length === 0) {
  ports.push(9239);
}
for (const port of ports) {
  killListenersOnPort(port);
}
// Never fail the launch — freeing is best-effort.
process.exit(0);
