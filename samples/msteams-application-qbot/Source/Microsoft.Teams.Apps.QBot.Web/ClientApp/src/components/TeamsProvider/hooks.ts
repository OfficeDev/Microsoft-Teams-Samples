import { ThemeInput } from '@fluentui/react-northstar';
import { Context } from '@microsoft/teams-js';
import { useContext, useMemo } from 'react';
import { TeamsContext, getTheme } from './TeamsProvider';

/**
 * Create a deferred version of the function that captures the parameters on execution, but only
 * calls the function after the provided promise resolves successfully.
 *
 * This is useful for ensuring 'initialization' calls complete before attempting to access API calls such
 * as the 'initialization' call for Microsoft Teams.
 */
function deferAfter<T extends (...args: any[]) => void>(
  promise: Promise<void>,
  fn: T,
) {
  return function (...params: Parameters<T>) {
    promise.then(() => {
      fn(...params);
    });
  };
}

/**
 * Create a hook for getting a callback for microsoft teams such as notifySuccess or executeDeeplink.
 * Hooks created through this call ensure that the teams context is initialized before attempting to
 * call the underlying callback.
 */
function createInitDeferredCallbackHook<T extends (...args: any[]) => void>(
  getter: (teams: typeof microsoftTeams) => T,
) {
  return function () {
    const providerContext = useContext(TeamsContext);
    const cb = getter(providerContext.microsoftTeams);
    return useMemo(() => deferAfter(providerContext.initializePromise, cb), [
      providerContext.initializePromise,
      cb,
    ]);
  };
}

export function useTeamsContext(): Context {
  const providerContext = useContext(TeamsContext);
  // trigger context refresh?
  // providerContext.microsoftTeams.getContext(providerContext.setContext);
  return providerContext.context;
}

export function useTheme(): ThemeInput {
  const ctx = useTeamsContext();
  return useMemo(() => getTheme(ctx.theme), [ctx.theme]);
}

export const useExecuteDeepLink = createInitDeferredCallbackHook(
  (teams) => teams.executeDeepLink,
);

export const useNotifyAppLoaded = createInitDeferredCallbackHook(
  (teams) => teams.appInitialization.notifyAppLoaded,
);

export const useNotifySuccess = createInitDeferredCallbackHook(
  (teams) => teams.appInitialization.notifySuccess,
);
