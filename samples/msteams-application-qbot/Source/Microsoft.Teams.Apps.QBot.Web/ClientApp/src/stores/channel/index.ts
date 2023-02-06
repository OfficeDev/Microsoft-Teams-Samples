import { Channel } from 'models';
import {
  createSlice,
  createEntityAdapter,
  PayloadAction,
} from '@reduxjs/toolkit';

import {
  createCommandHandler,
  iterableCommandHandler,
} from 'createCommandHandler';
import { ChannelService } from 'services/ChannelService';
import { globalErrorSlice } from 'stores/globalError';

export const channelAdapter = createEntityAdapter<Channel>({
  selectId: (channel) => channel.id,
  sortComparer: (a, b) => a.name.localeCompare(b.name),
});

const initialState = channelAdapter.getInitialState({
  loading: false,
});

export type ChannelState = typeof initialState;

export const channelSlice = createSlice({
  name: 'channels',
  initialState,
  reducers: {
    channelsLoading(state) {
      state.loading = true;
    },
    channelsLoaded(state, action: PayloadAction<{ channels: Channel[] }>) {
      state.loading = false;
      channelAdapter.upsertMany(state, action.payload.channels);
    },
  },
});

export function createChannelHandlers({
  channelService,
}: {
  channelService: ChannelService;
}) {
  async function* loadChannels(command: PayloadAction<{ courseId: string }>) {
    try {
      yield channelSlice.actions.channelsLoading();
      const channels = await channelService.loadChannelsForCourse(
        command.payload.courseId,
      );
      yield channelSlice.actions.channelsLoaded({ channels });
    } catch (error) {
      yield globalErrorSlice.actions.showError({
        error: error instanceof Error ? error : new Error(`${error}`),
      });
    }
  }
  return createCommandHandler({
    name: 'courseCommands',
    handlers: {
      loadChannels: iterableCommandHandler(loadChannels),
    },
  });
}
export type ChannelHandler = ReturnType<typeof createChannelHandlers>;
