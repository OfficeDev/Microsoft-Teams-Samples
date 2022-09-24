import { createSlice, PayloadAction } from '@reduxjs/toolkit';
import {
  createCommandHandler,
  iterableCommandHandler,
} from 'createCommandHandler';

type SliceState = { error: Error | undefined };

const initialState: SliceState = { error: undefined };

export const globalErrorSlice = createSlice({
  name: 'globalError',
  initialState,
  reducers: {
    clearError(state) {
      state.error = undefined;
    },
    showError(state, action: PayloadAction<{ error: Error }>) {
      state.error = action.payload.error;
    },
  },
});

export function createGlobalErrorHandlers() {
  function* setError(
    command: PayloadAction<{
      error: Error;
    }>,
  ) {
    const { error } = command.payload;
    yield globalErrorSlice.actions.showError({ error });
  }

  function* unSetError() {
    yield globalErrorSlice.actions.clearError();
  }

  return createCommandHandler({
    name: 'globalErrorCommands',
    handlers: {
      setError: iterableCommandHandler(setError),
      unSetError: iterableCommandHandler(unSetError),
    },
  });
}

export type GlobalErrorHandler = ReturnType<typeof createGlobalErrorHandlers>;
