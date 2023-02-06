import { KnowledgeBase } from 'models';
import {
  createSlice,
  createEntityAdapter,
  PayloadAction,
} from '@reduxjs/toolkit';

import {
  createCommandHandler,
  iterableCommandHandler,
} from 'createCommandHandler';
import { KnowledgeBaseService } from 'services/KnowledgeBaseService';
import { globalErrorSlice } from 'stores/globalError';

export const knowledgeBaseAdapter = createEntityAdapter<KnowledgeBase>({
  selectId: (knowledeBase) => knowledeBase.id,
  sortComparer: (a, b) => a.name.localeCompare(b.name),
});

const initialState = knowledgeBaseAdapter.getInitialState({
  loading: false,
  loaded: false,
});

export type KbState = typeof initialState;

export const knowledgeBaseSlice = createSlice({
  name: 'knowledgeBases',
  initialState,
  reducers: {
    setknowledgeBase: knowledgeBaseAdapter.setOne,
    deleteknowledgeBase: knowledgeBaseAdapter.removeOne,
    knowledgeBasesLoading: (state) => {
      state.loading = true;
    },
    knowledgeBasesLoaded(
      state,
      action: PayloadAction<{ knowledgeBases: KnowledgeBase[] }>,
    ) {
      state.loading = false;
      state.loaded = true;
      knowledgeBaseAdapter.upsertMany(state, action.payload.knowledgeBases);
    },
  },
});

export function createKnowledgeBaseHandlers({
  knowledgeBaseService,
}: {
  knowledgeBaseService: KnowledgeBaseService;
}) {
  async function* loadKnowledgeBase(command: PayloadAction<void>) {
    yield knowledgeBaseSlice.actions.knowledgeBasesLoading();
    try {
      const knowledgeBases = await knowledgeBaseService.loadKnowledgeBases();
      yield knowledgeBaseSlice.actions.knowledgeBasesLoaded({
        knowledgeBases,
      });
    } catch (error) {
      console.warn('Failed to load knowledgebases', { error });
      yield globalErrorSlice.actions.showError({
        error: error instanceof Error ? error : new Error(`${error}`),
      });
      yield knowledgeBaseSlice.actions.knowledgeBasesLoaded({
        knowledgeBases: [],
      });
    }
  }

  async function* createKnowledgeBase(
    command: PayloadAction<{
      knowledgeBase: Omit<KnowledgeBase, 'id' | 'userId'>;
    }>,
  ) {
    try {
      const newKb = await knowledgeBaseService.createKnowledgeBase(
        command.payload.knowledgeBase,
      );
      yield knowledgeBaseSlice.actions.setknowledgeBase(newKb);
    } catch (error) {
      console.warn('Failed to create knowledge base', { error });
      yield globalErrorSlice.actions.showError({
        error: error instanceof Error ? error : new Error(`${error}`),
      });
    }
  }

  async function* updateKnowledgeBase(
    command: PayloadAction<{ knowledgeBase: KnowledgeBase }>,
  ) {
    try {
      await knowledgeBaseService.updateKnowledgeBase(
        command.payload.knowledgeBase,
      );
      yield knowledgeBaseSlice.actions.setknowledgeBase(
        command.payload.knowledgeBase,
      );
    } catch (error) {
      console.warn('Failed to update knowledge base', { error });
      yield globalErrorSlice.actions.showError({
        error: error instanceof Error ? error : new Error(`${error}`),
      });
    }
  }

  async function* deleteKnowledgeBase(
    command: PayloadAction<{ knowledgeBase: KnowledgeBase }>,
  ) {
    try {
      await knowledgeBaseService.deleteKnowledgeBase(
        command.payload.knowledgeBase.id,
      );
      yield knowledgeBaseSlice.actions.deleteknowledgeBase(
        command.payload.knowledgeBase.id,
      );
    } catch (error) {
      console.warn('Failed to delete knowledge base', { error });
      yield globalErrorSlice.actions.showError({
        error: error instanceof Error ? error : new Error(`${error}`),
      });
    }
  }

  return createCommandHandler({
    name: 'kbCommands',
    handlers: {
      loadKnowledgeBases: iterableCommandHandler(loadKnowledgeBase),
      createKnowledgeBases: iterableCommandHandler(createKnowledgeBase),
      updateKnowledgeBases: iterableCommandHandler(updateKnowledgeBase),
      deleteKnowledgeBases: iterableCommandHandler(deleteKnowledgeBase),
    },
  });
}

export type KnowledgeBaseHandler = ReturnType<
  typeof createKnowledgeBaseHandlers
>;
