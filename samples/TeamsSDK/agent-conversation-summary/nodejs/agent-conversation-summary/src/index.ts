import { MessageActivityInput, stripMentionsText } from '@microsoft/teams.api';
import { App, type IActivityContext } from '@microsoft/teams.apps';
import { buildSummaryCard } from './card';
import { chatReply, summarizeThread, type ChatTurn } from './llm';
import { fetchRecentMessages, GraphPermissionError } from './graph';

const app = new App();
const MAX_HISTORY_TURNS = 10;

type SummaryResult = { ok: true; card: ReturnType<typeof buildSummaryCard> } | { ok: false; message: string };

async function runSummary(ctx: IActivityContext): Promise<SummaryResult> {
  try {
    const messages = await fetchRecentMessages(ctx);
    if (messages.length === 0) {
      return { ok: false, message: "I couldn't find any recent messages to summarize." };
    }
    const result = await summarizeThread(messages);
    return { ok: true, card: buildSummaryCard(result) };
  } catch (err) {
    if (err instanceof GraphPermissionError) {
      return { ok: false, message: err.message };
    }
    console.error(err);
    return { ok: false, message: 'Something went wrong while summarizing — check the logs.' };
  }
}

app.on('message', async (ctx) => {
  const { send, activity, storage } = ctx;
  const text = stripMentionsText(activity)?.trim() ?? '';
  await send({ type: 'typing' });

  if (/^summarize\b/i.test(text)) {
    const result = await runSummary(ctx);
    if (result.ok) {
      await send(new MessageActivityInput().addCard('adaptive', result.card));
    } else {
      await send(result.message);
    }
    return;
  }

  const key = activity.conversation.id;
  const history = ((await storage.get(key)) as ChatTurn[] | undefined) ?? [];
  history.push({ role: 'user', content: text });

  const reply = await chatReply(history);
  history.push({ role: 'assistant', content: reply });
  await storage.set(key, history.slice(-MAX_HISTORY_TURNS * 2));

  await send(reply);
});

app.on('card.action.refresh-summary', async (ctx) => {
  const result = await runSummary(ctx);
  return result.ok
    ? { statusCode: 200, type: 'application/vnd.microsoft.card.adaptive', value: result.card }
    : { statusCode: 200, type: 'application/vnd.microsoft.activity.message', value: result.message };
});

app.start(process.env.PORT || 3978).catch(console.error);