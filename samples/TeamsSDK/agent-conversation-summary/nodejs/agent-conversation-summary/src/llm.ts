import { z } from 'zod';

import Anthropic from '@anthropic-ai/sdk';

const anthropic = new Anthropic({ apiKey: process.env.ANTHROPIC_API_KEY });
const MODEL = 'claude-sonnet-5';

async function callChatCompletion(input: {
  system: string;
  messages: ChatTurn[];
}): Promise<string> {
  const response = await anthropic.messages.create({
    model: MODEL,
    max_tokens: 1024,
    system: input.system,
    messages: input.messages.map((m) => ({ role: m.role, content: m.content })),
  });
  const block = response.content.find((b) => b.type === 'text');
  return block?.type === 'text' ? block.text : '';
}

async function callStructuredCompletion<T>(input: {
  system: string;
  prompt: string;
  schema: z.ZodType<T>;
}): Promise<T> {
  const response = await anthropic.messages.create({
    model: MODEL,
    max_tokens: 1024,
    system: input.system,
    messages: [{ role: 'user', content: input.prompt }],
    tools: [{
      name: 'emit_result',
      description: 'Emit the structured result',
      input_schema: z.toJSONSchema(input.schema) as Anthropic.Tool.InputSchema,
    }],
    tool_choice: { type: 'tool', name: 'emit_result' },
  });
  const block = response.content.find((b) => b.type === 'tool_use');
  if (!block || block.type !== 'tool_use') throw new Error('Model did not return a structured result');
  return input.schema.parse(block.input);
}

const CHAT_SYSTEM_PROMPT =
  'You are a helpful assistant embedded in a Microsoft Teams bot. Keep replies concise and conversational.';

export type ChatTurn = { role: 'user' | 'assistant'; content: string };

export async function chatReply(history: ChatTurn[]): Promise<string> {
  return callChatCompletion({
    system: CHAT_SYSTEM_PROMPT,
    messages: history,
  });
}

const ThreadSummarySchema = z.object({
  summary: z.string().describe('2-4 sentence recap of what happened'),
  completed: z.array(z.string()),
  in_progress: z.array(z.string()),
  open: z.array(z.string()),
});
export type ThreadSummary = z.infer<typeof ThreadSummarySchema>;

export type ThreadMessage = { author: string; text: string; timestamp: string };

export async function summarizeThread(messages: ThreadMessage[]): Promise<ThreadSummary> {
  const transcript = messages
    .map((m) => `[${m.timestamp}] ${m.author}: ${m.text}`)
    .join('\n');

  return callStructuredCompletion({
    system:
      "You summarize Microsoft Teams conversations. Classify action items into completed, in_progress, or open based only on what's stated or clearly implied in the transcript.",
    prompt: `Transcript:\n${transcript}`,
    schema: ThreadSummarySchema,
  });
}