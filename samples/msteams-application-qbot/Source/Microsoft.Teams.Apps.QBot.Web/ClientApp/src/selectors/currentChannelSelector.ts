import { flow, memoize } from 'lodash';
import { createMatchSelector } from 'connected-react-router';
import { QBotState } from 'compositionRoot';

export const selectPathChannelId = flow(
  createMatchSelector<QBotState, { channelId?: string }>(
    '/:context/course/:courseId/channel/:channelId',
  ),
  memoize((match) => match?.params.channelId),
);

export const selectChannelIdFromPath = flow(
  createMatchSelector<QBotState, { channelId?: string }>(
    '/:context/courses/:courseId/channel/:channelId',
  ),
  memoize((match) => match?.params.channelId),
);
