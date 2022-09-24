import { flow, memoize } from 'lodash';
import { createMatchSelector } from 'connected-react-router';

import { QBotState } from 'compositionRoot';

// Don't use default export for selectors
// eslint-disable-next-line import/prefer-default-export
export const selectAppContext = flow(
  createMatchSelector<QBotState, { context?: string }>(
    '/:context(personal|taskModule|team)',
  ),
  memoize((match) => match?.params.context),
);
