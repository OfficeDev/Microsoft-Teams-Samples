import * as fs from "fs";
import * as os from "os";
import * as path from "path";
import { execSync } from "child_process";
import * as inspector from "inspector";
import { config } from "./config";
import { initFileLog, logEvent } from "./log";
import { SELF_TEST_ENABLED, TARGET_CONNECTIONS, logStatus, openInitialSockets, startStatusTimer } from "./socketPool";
import { runSelfTest } from "./selfTest";
import { startKeepAlive } from "./keepAlive";

// Single-instance guard. An orphaned bot process survives APX restarts (resilient reconnect) and
// would keep an extra connection registered in APX's directory — so an invoke could target the stale
// socket. Closing a terminal doesn't always kill a detached node, and the --inspect port then just
// collides while the orphan keeps its socket. A PID lockfile fixes it: on startup SIGTERM any prior
// instance, then claim the lock.
const LOCK_FILE = path.join(os.tmpdir(), "prkare-default-mode-socket-bot.lock");

async function enforceSingleInstance(): Promise<void> {
  try {
    let killed = false;
    if (fs.existsSync(LOCK_FILE)) {
      const prev = parseInt(fs.readFileSync(LOCK_FILE, "utf8").trim(), 10);
      if (prev && prev !== process.pid) {
        try {
          process.kill(prev, 0); // throws if the process is already gone (stale lock)
          process.kill(prev, "SIGTERM");
          killed = true;
          console.log(`[singleton] killed prior bot instance PID ${prev}`);
        } catch {
          /* stale lockfile — prior process already exited */
        }
      }
    }
    fs.writeFileSync(LOCK_FILE, String(process.pid));
    if (killed) {
      await sleep(300); // let the old socket fully close on Azure SignalR before we connect
    }
  } catch (e) {
    console.warn(`[singleton] could not enforce single instance: ${(e as Error).message}`);
  }
}

function releaseLock(): void {
  try {
    if (fs.existsSync(LOCK_FILE) && fs.readFileSync(LOCK_FILE, "utf8").trim() === String(process.pid)) {
      fs.unlinkSync(LOCK_FILE);
    }
  } catch {
    /* ignore */
  }
}

// The Node inspector (debugger) port from --inspect=<port> in dev; undefined for `npm start`.
function getInspectPort(): number | undefined {
  for (const arg of process.execArgv) {
    const m = arg.match(/^--inspect(?:-brk)?(?:=(?:.*:)?(\d+))?$/);
    if (m) {
      return m[1] ? Number(m[1]) : 9229;
    }
  }
  return undefined;
}

// Force-kill any OTHER process currently LISTENING on `port` (an orphaned prior run) so the port is
// free to reuse. Cross-platform: Windows netstat+taskkill /F, *nix lsof+kill -9.
function killListenersOnPort(port: number): void {
  const self = String(process.pid);
  try {
    if (process.platform === "win32") {
      const out = execSync("netstat -ano -p tcp", { encoding: "utf8", windowsHide: true });
      const pids = new Set<string>();
      for (const line of out.split(/\r?\n/)) {
        const m = line.match(new RegExp(`[:.]${port}\\b.*\\bLISTENING\\b\\s+(\\d+)\\s*$`, "i"));
        if (m && m[1] !== self) {
          pids.add(m[1]);
        }
      }
      for (const pid of pids) {
        try {
          execSync(`taskkill /F /PID ${pid}`, { stdio: "ignore", windowsHide: true });
          console.log(`[port] force-freed :${port} (killed PID ${pid})`);
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
            console.log(`[port] force-freed :${port} (killed PID ${pid})`);
          } catch {
            /* already gone */
          }
        }
      }
    }
  } catch (e) {
    console.warn(`[port] could not inspect/free :${port}: ${(e as Error).message}`);
  }
}

// If a prior run still holds the inspector port, force-free it and re-open the debugger on it, so a
// relaunch never dies with "address already in use" and VS Code can still attach — no manual kill.
function freeInspectorPortIfBusy(): void {
  const port = getInspectPort();
  if (port === undefined) {
    return; // not launched with --inspect (e.g. `npm start`); nothing to manage
  }
  if (inspector.url()) {
    return; // our own inspector bound fine; the port was not contended
  }

  console.warn(`[inspector] port ${port} was busy at startup; force-freeing it from any prior run.`);
  killListenersOnPort(port);

  try {
    inspector.open(port, "127.0.0.1", false);
    console.log(`[inspector] reopened on 127.0.0.1:${port}`);
  } catch (e) {
    console.warn(`[inspector] could not reopen on ${port}: ${(e as Error).message}`);
  }
}

async function main(): Promise<void> {
  const logFile = initFileLog();
  console.log(`[log] writing this run to ${logFile}`);

  if (!config.botKey) {
    console.error(
      "Bot identity is required: set BOT_ID (filled by Teams Toolkit provision) or BOT_KEY in .env / " +
        ".localConfigs. Exiting."
    );
    process.exit(1);
  }

  await enforceSingleInstance();
  freeInspectorPortIfBusy();

  console.log(
    `prkare-default-mode-socket-bot starting. apx=${config.apxBaseUrl} botKey=${config.botKey} ` +
      `defaultDirective=${config.defaultDirective}`
  );

  logEvent("bot", {
    phase: "start",
    sockets: TARGET_CONNECTIONS,
    selfTest: SELF_TEST_ENABLED ? "on (limit probe, refresh scenarios, same-token checks)" : "off (SOCKET_SELF_TEST=false)",
  });

  // Open the sockets one after the other, run the startup self-test once, then keep the sockets alive:
  // refresh each one 60 s before its token expires (replaces header + drain) and reconnect after a drop.
  if (!(await openInitialSockets())) {
    console.error("[bot] could not open the sockets. Exiting.");
    process.exit(1);
  }
  if (SELF_TEST_ENABLED) {
    await runSelfTest();
  }
  startKeepAlive();
  logEvent("bot", { phase: "ready", next: "use the bot from Teams; each delivery logs a [route <<] line with the socket it arrived on" });
  startStatusTimer();
}

function sleep(ms: number): Promise<void> {
  return new Promise((r) => setTimeout(r, ms));
}

process.on("exit", (code) => {
  logStatus(`exit code=${code}`);
  releaseLock();
});
process.on("SIGINT", () => {
  console.log("\nShutting down.");
  releaseLock();
  process.exit(0);
});
process.on("SIGTERM", () => {
  releaseLock();
  process.exit(0);
});

void main();
