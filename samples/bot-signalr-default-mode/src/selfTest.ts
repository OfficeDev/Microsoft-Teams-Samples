import { HubConnectionState } from "@microsoft/signalr";
import { NegotiateError, REPLACES_HEADER } from "./negotiate";
import { openSocket, SocketSession } from "./socketClient";
import { logEvent, logError } from "./log";
import {
  APX_REPLACED_REASON,
  TARGET_CONNECTIONS,
  callbacks,
  closeByBot,
  describeConnectError,
  drainAndClose,
  drainingCount,
  inGroupCount,
  liveCount,
  nextLabel,
  openFresh,
  receivedWhileRetired,
  record,
  results,
  retire,
  seconds,
  sleep,
  slots,
  STEP_GAP_MS,
  track,
} from "./socketPool";

// Startup self-test, run once after the sockets are open. Every step is logged under the [flow] tag with its
// expected outcome, and a [summary] table (expected vs actual per step) closes the run:
//   1. Probes APX's per-bot connection limit with extra connects until one is refused (429), then closes the
//      extra sockets.
//   2. Refreshes every socket the old way (no header): opens the replacement, stops the old socket a fixed 5 s
//      later. For the first one it also tries a second refresh while the old socket is still open.
//   3. Refreshes every socket the new way (x-ms-socket-replaces-connection-id): the first and third drain and
//      close their old socket; the second leaves it open for APX to close, and the third runs while that one
//      is still draining.
// The same-token check runs after every new socket (socketPool.openFresh).

// APX's DefaultMaxConnectionsPerBot, used only to log the expected outcome. ECS
// DeliverEventViaSocketMaxConnectionsPerBot overrides the real limit.
const APX_DEFAULT_MAX_CONNECTIONS = 3;

// Extra connects made after the target is reached, stopping at the first refusal.
const MAX_LIMIT_PROBES = 3;
const MAX_PROBE_ATTEMPTS = 3;

// Old-way refresh: SDKs today stop the old socket a fixed 5 s after the new one is ready.
const LEGACY_STOP_DELAY_MS = 5000;

// APX closes a replaced socket still open 60 s after the new socket was ready; wait a bit longer than that.
const APX_REPLACED_CLOSE_MS = 60000;
const APX_REPLACED_CLOSE_WAIT_MS = 90000;

export async function runSelfTest(): Promise<void> {
  logEvent("flow", { phase: "self-test: connection limit probe" });
  await probeLimit();

  logEvent("flow", { phase: "self-test: refresh the old way (no header)" });
  for (let slot = 1; slot <= TARGET_CONNECTIONS; slot++) {
    await legacyRefresh(slot, slot === 1 && TARGET_CONNECTIONS > 1);
    await sleep(STEP_GAP_MS);
  }

  logEvent("flow", { phase: `self-test: refresh the new way (${REPLACES_HEADER})` });
  const leftOpen: { old: SocketSession; fresh: SocketSession }[] = [];
  for (let slot = 1; slot <= TARGET_CONNECTIONS; slot++) {
    const leaveOldOpen = slot === 2;
    const outcome = await headerRefresh(slot, leaveOldOpen);
    if (outcome && leaveOldOpen) {
      // No gap: the next refresh runs while this old socket is still draining.
      leftOpen.push(outcome);
    } else {
      await sleep(STEP_GAP_MS);
    }
  }
  for (const { old, fresh } of leftOpen) {
    await waitForApxClose(old, fresh);
  }

  logSummary();
}

// APX refuses a connect only when the bot already has MORE than the limit open (it keeps one spare slot so
// a refreshing bot can open its replacement before closing the old socket). With the default limit of 3:
// connect #4 is accepted (APX outcome spare_slot) and connect #5 gets 429 + Retry-After.
async function probeLimit(): Promise<void> {
  logEvent("limit-probe", {
    phase: "start",
    liveBefore: liveCount(),
    rule: "APX refuses only when live > max (one spare slot)",
    expectedWithDefaultMax3: "connect #4 -> 200 (spare_slot), connect #5 -> 429 + Retry-After 15",
    note: "ECS DeliverEventViaSocketMaxConnectionsPerBot overrides max=3",
  });

  const probes: SocketSession[] = [];
  let refused = false;
  for (let n = 1; n <= MAX_LIMIT_PROBES && !refused; n++) {
    const { index, label } = nextLabel();
    const connectNo = TARGET_CONNECTIONS + n;
    const liveBefore = liveCount();
    const expected = liveBefore > APX_DEFAULT_MAX_CONNECTIONS ? "REFUSED" : "ACCEPTED";
    let opened: SocketSession | undefined;
    for (let attempt = 1; attempt <= MAX_PROBE_ATTEMPTS && !opened && !refused; attempt++) {
      logEvent("limit-probe >>", { conn: label, connectNo, attempt, liveBefore, expectedWithDefaultMax3: expected });
      try {
        opened = await openFresh(index, label, { slot: 0 });
        probes.push(opened);
        const asExpected = expected === "ACCEPTED";
        record(`limit probe connect #${connectNo}`, expected, "ACCEPTED", asExpected);
        (asExpected ? logEvent : logError)("limit-probe <<", {
          conn: label,
          connectNo,
          result: "ACCEPTED",
          connectionId: opened.connectionId,
          liveNow: liveCount(),
          expectedWithDefaultMax3: expected,
          asExpected,
          note: liveBefore === APX_DEFAULT_MAX_CONNECTIONS ? "this is the spare slot (APX outcome=spare_slot)" : undefined,
        });
      } catch (e) {
        if (e instanceof NegotiateError && e.status === 429) {
          refused = true;
          const asExpected = expected === "REFUSED";
          record(`limit probe connect #${connectNo}`, expected, "REFUSED (429)", asExpected);
          (asExpected ? logEvent : logError)(
            "limit-probe <<",
            {
              conn: label,
              connectNo,
              result: "REFUSED",
              status: e.status,
              retryAfterSec: e.retryAfterSec,
              liveBefore,
              expectedWithDefaultMax3: expected,
              asExpected,
              body: e.body,
            },
            e.cv
          );
        } else {
          logError("limit-probe <<", { conn: label, connectNo, attempt, result: "FAILED (not a limit refusal)", error: describeConnectError(e) });
          if (attempt < MAX_PROBE_ATTEMPTS) {
            await sleep(2000);
          }
        }
      }
    }
    if (!opened && !refused) {
      logError("limit-probe", { phase: "inconclusive", conn: label, note: `connect #${connectNo} failed ${MAX_PROBE_ATTEMPTS} times for a reason other than 429` });
      break;
    }
  }
  if (!refused) {
    logError("limit-probe", { phase: "no 429 seen", extraConnects: probes.length, note: "the limit is higher than expected (check ECS DeliverEventViaSocketMaxConnectionsPerBot)" });
  }

  // Close the extra sockets so exactly TARGET_CONNECTIONS remain.
  for (const probe of probes) {
    await closeByBot(probe, `limit probe cleanup (keep ${TARGET_CONNECTIONS})`);
  }
  if (probes.length > 0) {
    await sleep(STEP_GAP_MS);
  }
  logEvent("limit-probe", { phase: "done", live: liveCount() });
}

// Refresh, old way: no header. APX counts both sockets, and the spare slot lets the bot at its limit open the
// replacement. The old socket stays in the bot's group (and keeps getting deliveries) until it closes.
async function legacyRefresh(slot: number, tryContrast: boolean): Promise<void> {
  const old = slots[slot - 1];
  const { index, label } = nextLabel();
  const step = `A${slot}: refresh slot ${slot} the old way`;
  const inGroupBefore = inGroupCount();
  const expected = "200";
  logEvent("flow", {
    step,
    action: `open ${label} to replace ${old.label} (no header)`,
    oldConn: old.label,
    oldConnectionId: old.connectionId,
    inGroupBefore,
    expected: inGroupBefore >= APX_DEFAULT_MAX_CONNECTIONS
      ? "200 (APX spare slot: live == max; APX log outcome=spare_slot)"
      : "200 (under the limit)",
  });

  let fresh: SocketSession;
  try {
    fresh = await openFresh(index, label, { slot });
  } catch (e) {
    const actual = describeConnectError(e);
    record(step, expected, actual, false);
    logError("refresh <<", { mode: "legacy", slot, newConn: label, oldConn: old.label, result: "FAILED", error: actual }, (e as NegotiateError)?.cv);
    return;
  }
  record(step, expected, "200", true);
  retire(old, fresh, "legacy");
  logEvent("refresh <<", {
    mode: "legacy",
    slot,
    result: "OK",
    newConn: fresh.label,
    newConnectionId: fresh.connectionId,
    oldConn: old.label,
    oldConnectionId: old.connectionId,
    inGroupNow: inGroupCount(),
    note: "old socket is still in the bot's group and can still get deliveries",
  });

  if (tryContrast) {
    await legacyRefreshWhileOldOpen(old);
  }

  // Old SDK behavior: stop the old socket a fixed 5 s after the new one is ready, without waiting for in-flight work.
  const waitMs = Math.max(0, fresh.readyAtMs + LEGACY_STOP_DELAY_MS - Date.now());
  logEvent("flow", { step, action: `close ${old.label} ${LEGACY_STOP_DELAY_MS / 1000}s after ${fresh.label} was ready (old SDK: fixed delay, no drain)`, waitMs });
  await sleep(waitMs);
  if (old.inFlight > 0) {
    logError("drain", { conn: old.label, event: "closing with deliveries still in flight (old SDK behavior)", inFlight: old.inFlight });
  }
  await closeByBot(old, `old-way refresh: replaced by ${fresh.label}`);
}

// While the old socket of a refresh is still open the bot is at max + 1, so a second old-way refresh must be
// refused until the old socket closes.
async function legacyRefreshWhileOldOpen(oldStillOpen: SocketSession): Promise<void> {
  const other = slots.find((s) => s && s.slot !== oldStillOpen.slot);
  if (!other) {
    return;
  }
  const { index, label } = nextLabel();
  const step = `A${oldStillOpen.slot}-contrast: refresh slot ${other.slot} the old way while ${oldStillOpen.label} is still open`;
  const inGroupBefore = inGroupCount();
  const expected = inGroupBefore > APX_DEFAULT_MAX_CONNECTIONS ? "429" : "200";
  logEvent("flow", {
    step,
    action: `open ${label} to replace ${other.label} (no header) before closing ${oldStillOpen.label}`,
    inGroupBefore,
    expected: expected === "429" ? `429 + Retry-After (live ${inGroupBefore} > max ${APX_DEFAULT_MAX_CONNECTIONS})` : "200",
  });
  try {
    const extra = await openSocket(index, label, callbacks, { slot: other.slot });
    track(extra);
    record(step, expected, "200", expected === "200");
    (expected === "200" ? logEvent : logError)("refresh <<", { mode: "legacy", step: "contrast", result: "ACCEPTED", conn: label, connectionId: extra.connectionId, expected, note: "closing it again; this test keeps one socket per slot" });
    await closeByBot(extra, "contrast refresh cleanup");
  } catch (e) {
    const status = e instanceof NegotiateError ? String(e.status) : "error";
    const asExpected = status === expected;
    record(step, expected, describeConnectError(e), asExpected);
    (asExpected ? logEvent : logError)(
      "refresh <<",
      {
        mode: "legacy",
        step: "contrast",
        result: status === "429" ? "REFUSED" : "FAILED",
        conn: label,
        status,
        retryAfterSec: e instanceof NegotiateError ? e.retryAfterSec : undefined,
        body: e instanceof NegotiateError ? e.body : (e as Error).message,
        expected,
        asExpected,
      },
      e instanceof NegotiateError ? e.cv : undefined
    );
  }
}

// Refresh, new way: x-ms-socket-replaces-connection-id names the old socket. APX doesn't count it against the
// limit, puts a `replaces` claim on the new token, and once the new socket is ready moves the old one to the
// bot's draining group (no new deliveries). It closes the old socket if it is still open 60 s later.
async function headerRefresh(slot: number, leaveOldOpen: boolean): Promise<{ old: SocketSession; fresh: SocketSession } | undefined> {
  const old = slots[slot - 1];
  const { index, label } = nextLabel();
  const step = `B${slot}: refresh slot ${slot} the new way`;
  const inGroupBefore = inGroupCount();
  const drainingBefore = drainingCount();
  const expected = "200 + replaces claim";
  logEvent("flow", {
    step,
    action: `open ${label} with ${REPLACES_HEADER}: ${old.connectionId} (replacing ${old.label})`,
    inGroupBefore,
    drainingBefore,
    expected:
      `200; token has replaces=${old.connectionId}; APX doesn't count ${old.label} (live ${inGroupBefore - 1}, draining ${drainingBefore}, ` +
      `limit ${APX_DEFAULT_MAX_CONNECTIONS} each) and moves it to the draining group when ${label} is ready (APX log: BotHub REPLACED)`,
    oldSocket: leaveOldOpen
      ? `left open: APX should close it ~${APX_REPLACED_CLOSE_MS / 1000}s after ${label} is ready`
      : `drained, then closed by the bot (nothing in flight and >= 5s after ${label} is ready)`,
  });

  let fresh: SocketSession;
  try {
    fresh = await openFresh(index, label, { slot, replacesConnectionId: old.connectionId });
  } catch (e) {
    const actual = describeConnectError(e);
    record(step, expected, actual, false);
    logError("refresh <<", { mode: "header", slot, newConn: label, oldConn: old.label, result: "FAILED", error: actual }, (e as NegotiateError)?.cv);
    return undefined;
  }
  const claimMatches = fresh.negotiate.replacesClaim === old.connectionId;
  record(step, expected, `200, replaces claim ${claimMatches ? "matches" : `= ${fresh.negotiate.replacesClaim ?? "(absent)"}`}`, claimMatches);
  retire(old, fresh, "header");
  (claimMatches ? logEvent : logError)("refresh <<", {
    mode: "header",
    slot,
    result: "OK",
    newConn: fresh.label,
    newConnectionId: fresh.connectionId,
    oldConn: old.label,
    oldConnectionId: old.connectionId,
    tokenReplacesClaim: fresh.negotiate.replacesClaim ?? "(absent)",
    claimMatchesOld: claimMatches,
    inGroupNow: inGroupCount(),
    drainingNow: drainingCount(),
    note: "from now on APX should send nothing to the old socket; anything that still arrives there is logged with retired=header",
  });

  if (leaveOldOpen) {
    logEvent("flow", { step, action: `leave ${old.label} open for APX to close`, expectedCloseAt: new Date(fresh.readyAtMs + APX_REPLACED_CLOSE_MS).toISOString() });
    return { old, fresh };
  }
  await drainAndClose(old, fresh);
  return undefined;
}

// The old socket of a header refresh that the bot left open: APX should close it ~60 s after the new socket was
// ready. APX passes the reason "Replaced by a newer socket." to Azure SignalR, but the JS SignalR client gets a
// plain disconnect without it, so only the timing is checked.
async function waitForApxClose(old: SocketSession, fresh: SocketSession): Promise<void> {
  const step = `B${old.slot}-apx-close: APX closes ${old.label} left open after the refresh`;
  const expected = `closed by server ~${APX_REPLACED_CLOSE_MS / 1000}s after ${fresh.label} was ready`;
  logEvent("flow", { step, action: `wait up to ${APX_REPLACED_CLOSE_WAIT_MS / 1000}s after ${fresh.label} was ready`, expected });
  const giveUpAt = fresh.readyAtMs + APX_REPLACED_CLOSE_WAIT_MS;
  while (old.connection.state !== HubConnectionState.Disconnected && Date.now() < giveUpAt) {
    await sleep(500);
  }
  if (old.connection.state !== HubConnectionState.Disconnected) {
    record(step, expected, `still open ${seconds(Date.now() - fresh.readyAtMs)}s after ${fresh.label} was ready`, false);
    logError("flow", { step, result: "STILL OPEN", note: "APX did not close the replaced socket; the bot closes it now" });
    await closeByBot(old, "APX did not close the replaced socket");
    return;
  }
  const afterMs = (old.closedAtMs ?? Date.now()) - fresh.readyAtMs;
  const asExpected = !old.closedByBot && Math.abs(afterMs - APX_REPLACED_CLOSE_MS) <= 10000;
  const reasonSeen = (old.closeError ?? "").includes(APX_REPLACED_REASON);
  record(step, expected, `closed by ${old.closedByBot ? "bot" : "server"} ${seconds(afterMs)}s after ${fresh.label} was ready; close reason ${reasonSeen ? "seen" : "not visible to the client"}`, asExpected);
  (asExpected ? logEvent : logError)("flow", {
    step,
    result: "CLOSED",
    closedBy: old.closedByBot ? "bot" : "server",
    secondsAfterNewSocketReady: seconds(afterMs),
    closeError: old.closeError ?? "(none)",
    reasonVisibleToClient: reasonSeen,
    receivedWhileDraining: receivedWhileRetired(old),
    asExpected,
  });
}

function logSummary(): void {
  const failed = results.filter((r) => !r.asExpected).length;
  (failed === 0 ? logEvent : logError)("summary", { steps: results.length, asExpected: results.length - failed, notAsExpected: failed });
  for (const r of results) {
    (r.asExpected ? logEvent : logError)("summary", { step: r.step, expected: r.expected, actual: r.actual, result: r.asExpected ? "AS EXPECTED" : "NOT AS EXPECTED" });
  }
}
