import { QBotState } from 'compositionRoot';

export const selectGlobalErrorState = (state: QBotState): boolean =>
  state.globalError.error ? true : false;

export const selectGlobalErrorText = (state: QBotState): string =>
  (state.globalError.error && state.globalError.error.message) || '';
