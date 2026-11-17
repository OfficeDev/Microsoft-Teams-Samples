import type { IActivityContext } from '@microsoft/teams.apps';
import { GraphError } from '@microsoft/teams.graph';
import type { ThreadMessage } from './llm';

export class GraphPermissionError extends Error {}

function stripHtml(html: string): string {
  return html.replace(/<[^>]+>/g, '').trim();
}

export async function fetchRecentMessages(
  ctx: IActivityContext,
  limit = 50,
): Promise<ThreadMessage[]> {
  const { activity } = ctx;
  const isChannel = activity.conversation.conversationType === 'channel';

  const url = isChannel
    ? `/teams/${activity.team?.aadGroupId}/channels/${activity.channel?.id}/messages`
    : `/chats/${activity.conversation.id}/messages`;

  try {
    const { data } = await ctx.appGraph.http.get(url, { params: { $top: limit } });
    return (data.value ?? [])
      .filter((m: any) => m.body?.content)
      .map((m: any) => ({
        author: m.from?.user?.displayName ?? 'Unknown',
        text: stripHtml(m.body.content),
        timestamp: m.createdDateTime,
      }))
      .reverse(); // Graph returns newest-first; chronological order for the transcript
  } catch (err) {
    if (err instanceof GraphError && (err.statusCode === 403 || err.statusCode === 401)) {
      throw new GraphPermissionError(
        isChannel
          ? "I don't have permission to read this channel yet — grant the app ChannelMessage.Read.All (or the RSC ChannelMessage.Read.Group permission) and get admin consent."
          : "I don't have permission to read this chat yet — grant the app Chat.Read.All and get admin consent.",
      );
    }
    throw err;
  }
}