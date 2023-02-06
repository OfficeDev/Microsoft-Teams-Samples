import { QBotState } from 'compositionRoot';
import { flow, memoize } from 'lodash';
import { channelAdapter } from 'stores/channel';

const channelSelectors = channelAdapter.getSelectors<QBotState>(
  (state) => state.channels,
);
export const selectChannels = channelSelectors.selectAll;
export const selectChannelsById = flow(
  selectChannels,
  memoize((channels) => new Map(channels.map((c) => [c.id, c]))),
);
