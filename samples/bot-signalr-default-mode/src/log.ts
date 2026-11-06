// Structured, greppable logging for every call in/out of the bot. Each line is:
//   [<ISO-8601 UTC>] [<tag>] key=value ...  MS-CV=<cv>
// so you can copy the MS-CV + timestamp straight into the APX log search. MS-CV is the correlation
// vector: for an inbound socket envelope it's echoed on the envelope (env.cv); for an outbound HTTP
// call it's on the APX *response* headers (APX mints it per request). Both success and failure are
// logged with the same shape.
import * as fs from "fs";
import * as path from "path";

export function nowIso(): string {
  return new Date().toISOString();
}

let logFilePath: string | undefined;

// Tees everything written to the console (the structured lines below, the SignalR client's own log and
// any other output) into logs/socket-test-<UTC start time>.log, so a whole run can be reviewed later.
// Tokens are redacted in the file.
export function initFileLog(): string {
  if (logFilePath) {
    return logFilePath;
  }
  const dir = path.resolve(__dirname, "..", "logs");
  fs.mkdirSync(dir, { recursive: true });
  logFilePath = path.join(dir, `socket-test-${nowIso().replace(/[:.]/g, "-")}.log`);
  for (const method of ["log", "info", "warn", "error"] as const) {
    const original = console[method].bind(console);
    console[method] = (...args: unknown[]): void => {
      original(...args);
      appendToFile(args);
    };
  }
  return logFilePath;
}

function appendToFile(args: unknown[]): void {
  if (!logFilePath) {
    return;
  }
  let line = args.map(stringify).join(" ");
  if (!/^\[\d{4}-\d{2}-\d{2}T/.test(line)) {
    line = `[${nowIso()}] ${line}`;
  }
  try {
    fs.appendFileSync(logFilePath, redactSecrets(line) + "\n");
  } catch {
    /* never let file logging break the bot */
  }
}

function stringify(a: unknown): string {
  if (typeof a === "string") {
    return a;
  }
  if (a instanceof Error) {
    return a.stack ?? a.message;
  }
  try {
    return JSON.stringify(a);
  } catch {
    return String(a);
  }
}

function redactSecrets(s: string): string {
  return s
    .replace(/access_token=[^&\s"]+/gi, "access_token=<redacted>")
    .replace(/Bearer\s+[A-Za-z0-9._~+/=-]+/g, "Bearer <redacted>")
    .replace(/eyJ[A-Za-z0-9_-]+\.[A-Za-z0-9_-]+\.[A-Za-z0-9_-]+/g, "<jwt redacted>");
}

// Render a flat field bag as `key=value` pairs, skipping undefined/null. Values are stringified and
// whitespace-collapsed so each event stays on one grep-able line.
function fields(bag: Record<string, unknown>): string {
  const parts: string[] = [];
  for (const [k, v] of Object.entries(bag)) {
    if (v === undefined || v === null) continue;
    let s = typeof v === "string" ? v : JSON.stringify(v);
    s = String(s).replace(/\s+/g, " ");
    parts.push(`${k}=${s}`);
  }
  return parts.join(" ");
}

// One event line. `cv` is rendered last (and always, even when empty) so MS-CV is easy to spot/grep.
export function logEvent(tag: string, bag: Record<string, unknown>, cv?: string): void {
  const line = `[${nowIso()}] [${tag}] ${fields(bag)}  MS-CV=${cv ?? "(none)"}`;
  console.log(line);
}

export function logError(tag: string, bag: Record<string, unknown>, cv?: string): void {
  const line = `[${nowIso()}] [${tag}] ${fields(bag)}  MS-CV=${cv ?? "(none)"}`;
  console.error(line);
}

// Extract the MS-CV from a response's headers (case-insensitive).
export function cvOf(h: Headers | undefined): string | undefined {
  if (!h) return undefined;
  return h.get("MS-CV") ?? h.get("ms-cv") ?? undefined;
}

// Correlation/diagnostic headers useful for looking a request up in the APX server logs.
export function diagHeaders(h: Headers | undefined): string {
  if (!h) return "[no headers]";
  const wanted = [
    "MS-CV",
    "Date",
    "X-MSEdge-Ref",
    "x-ms-request-id",
    "x-ms-correlation-request-id",
    "request-id",
    "apim-request-id",
    "x-ms-timestamp",
  ];
  const parts: string[] = [];
  for (const name of wanted) {
    const v = h.get(name);
    if (v) parts.push(`${name}=${v}`);
  }
  return parts.length ? `[${parts.join("  ")}]` : "[no correlation headers]";
}

// Truncate long payloads so a single event stays readable but still shows the shape.
export function truncate(s: string | undefined, max = 1200): string {
  if (s == null) return "(none)";
  return s.length <= max ? s : s.slice(0, max) + `...(+${s.length - max} chars)`;
}
