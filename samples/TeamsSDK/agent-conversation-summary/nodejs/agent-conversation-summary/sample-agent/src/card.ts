import { AdaptiveCard, Container, ContainerStyle, ExecuteAction, TextBlock, TextColor } from '@microsoft/teams.cards';
import type { ThreadSummary } from './llm';

function statusSection(
  title: string,
  items: string[],
  style: ContainerStyle,
  color: TextColor,
): Container {
  const body = items.length ? items.map((item) => `• ${item}`).join('\n\n') : '_(none)_';
  return new Container(
    new TextBlock(title).withWeight('Bolder').withColor(color),
    new TextBlock(body).withWrap(true),
  ).withStyle(style);
}

export function buildSummaryCard(summary: ThreadSummary): AdaptiveCard {
  return new AdaptiveCard(
    new TextBlock('Thread Summary').withWeight('Bolder').withSize('Large'),
    new TextBlock(summary.summary).withWrap(true),
    statusSection(`✅ Completed (${summary.completed.length})`, summary.completed, 'good', 'Good'),
    statusSection(`🔄 In Progress (${summary.in_progress.length})`, summary.in_progress, 'warning', 'Warning'),
    statusSection(`🕐 Open (${summary.open.length})`, summary.open, 'attention', 'Attention'),
  ).withActions(
    new ExecuteAction().withTitle('🔄 Refresh').withData({ action: 'refresh-summary' }),
  );
}