// Teams requires the user to @-mention a bot in channels and group chats, so the inbound text arrives as
// "<at>Prkare Socket Test</at> hi". APX delivers the raw Bot Framework activity over the socket, so there
// is no TurnContext here — this is the equivalent of TurnContext.removeRecipientMention for raw activities.

export type ConversationScope = "personal" | "groupchat" | "channel" | "unknown";

export interface MentionEntity {
  type?: string;
  text?: string;
  mentioned?: { id?: string; name?: string };
}

// conversationType is "personal" | "groupChat" | "channel". A meeting's chat is a groupChat, so meetings
// need no separate branch — they are covered by the groupchat scope in the manifest.
export function getConversationScope(activity: any): ConversationScope {
  const raw = String(activity?.conversation?.conversationType ?? "").toLowerCase();
  if (raw === "personal") return "personal";
  if (raw === "groupchat" || raw === "group") return "groupchat";
  if (raw === "channel") return "channel";
  return "unknown";
}

export function isGroupScope(scope: ConversationScope): boolean {
  return scope === "groupchat" || scope === "channel";
}

export function getMentions(activity: any): MentionEntity[] {
  const entities = Array.isArray(activity?.entities) ? activity.entities : [];
  return entities.filter((e: any) => String(e?.type ?? "").toLowerCase() === "mention");
}

export function isBotMentioned(activity: any): boolean {
  const recipientId = activity?.recipient?.id;
  if (!recipientId) return false;
  return getMentions(activity).some((m) => idsMatch(m?.mentioned?.id, recipientId));
}

// Strips every <at>..</at> tag that names the bot, leaving the user's actual input. A 1:1 message carries
// no bot mention entity, so this is a no-op and the raw text falls through unchanged.
export function cleanUserText(activity: any): string {
  let text = String(activity?.text ?? "");
  if (!text) return "";

  const recipientId = activity?.recipient?.id;
  for (const mention of getMentions(activity)) {
    if (!idsMatch(mention?.mentioned?.id, recipientId)) continue;

    if (mention.text) {
      text = replaceAll(text, mention.text, " ");
    } else if (mention.mentioned?.name) {
      // Some clients omit entity.text; fall back to the <at>Name</at> form for that same mention.
      text = text.replace(atTagPattern(mention.mentioned.name), " ");
    }
  }

  // Entities can be missing entirely (or lag a rename) while the tag is still in the text. Only the bot's
  // own tag is removed — mentions of other people are left intact so the echo still shows them.
  const botName = activity?.recipient?.name;
  if (botName) {
    text = text.replace(atTagPattern(botName), " ");
  }

  return collapse(text);
}

function atTagPattern(name: string): RegExp {
  return new RegExp(`<at\\b[^>]*>\\s*${escapeRegExp(name)}\\s*</at>`, "gi");
}

// Teams bot ids are "28:<appId>"; compare case-insensitively and tolerate one side carrying the prefix.
function idsMatch(left: string | undefined, right: string | undefined): boolean {
  const a = normalizeId(left);
  const b = normalizeId(right);
  return a.length > 0 && a === b;
}

function normalizeId(value: string | undefined): string {
  const trimmed = String(value ?? "").trim().toLowerCase();
  return trimmed.startsWith("28:") ? trimmed.slice(3) : trimmed;
}

function escapeRegExp(value: string): string {
  return value.replace(/[.*+?^${}()|[\]\\]/g, "\\$&");
}

// String.replaceAll is ES2021; this project targets ES2020.
function replaceAll(haystack: string, needle: string, replacement: string): string {
  if (!needle) return haystack;
  return haystack.split(needle).join(replacement);
}

function collapse(text: string): string {
  return text.replace(/\s+/g, " ").trim();
}
