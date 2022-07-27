import { useSelector } from 'react-redux';
import { CallHistoryMethodAction, push } from 'connected-react-router';

import { selectAppContext } from 'selectors';
import { LocationState, Path } from 'history';
import { useCallback } from 'react';

/**
 * Hook for getting a `push` function that preserves the first segment in the
 * path defined by the 'appContext' selector
 * @param  {S|undefined} state?
 * @param  {} =>CallHistoryMethodAction<[string
 * @param  {} (S|undefined
 */
export default function usePushRelativePath(): <S = unknown>(
  path: Path,
  state?: S | undefined,
) => CallHistoryMethodAction<[string, (S | undefined)?]> {
  const appContext = useSelector(selectAppContext);
  return useCallback(
    <S = LocationState>(path: Path, state?: S) => {
      if (appContext === undefined) {
        return push<S>(path, state);
      } else {
        const nextPath = `/${appContext}${path}`;
        return push<S>(nextPath, state);
      }
    },
    [appContext],
  );
}
