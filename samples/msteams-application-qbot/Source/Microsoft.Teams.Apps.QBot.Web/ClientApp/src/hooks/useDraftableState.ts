/* eslint-disable @typescript-eslint/no-explicit-any */
import { useState, Dispatch, SetStateAction, useCallback } from 'react';
import { produce, Draft } from 'immer';

/**
 * The useDraft hook is a decorator hook for wrappinng the standard [state, setState]
 * in an immer.js produce function in a safe way to avoid excess callback creation and
 * provide a terser experience.
 *
 * For more information, see the documentation for immer.js' produce
 * https://immerjs.github.io/immer/produce
 * @param state the state provided by the React useState hook
 * @param setState the setState function provided by the React useState hook
 * @returns A produce hook which takes a callback for producing the next state.
 *
 * @example
 *  const [user, setUser] = useState<{id: string, name: string}>({id: '', name: ''});
 *  const produceUser = useDraft(user, setUser);
 *  const setUserName = useCallback((nextName: string) => produceUser(user => user.name = nextName, []));
 */
export function useDraft<S>(
  state: S,
  setState: Dispatch<SetStateAction<S>>,
): (producer: (draft: Draft<S>) => void) => void {
  return useCallback(
    (producer: (draft: Draft<S>) => void) => {
      const nextValue = produce(state, producer);
      setState(nextValue);
    },
    // eslint-disable-next-line react-hooks/exhaustive-deps
    [state, setState],
  );
}

type DraftCallback<S> = (...args: any[]) => (draft: Draft<S>) => void;
export function useDraftCallback<S>(
  state: S,
  setState: Dispatch<SetStateAction<S>>,
): <T extends DraftCallback<S>>(
  callback: T,
  deps: readonly any[],
) => (...args: Parameters<T>) => void {
  return function useProduceCallback<T extends DraftCallback<S>>(
    callback: T,
    deps: readonly any[],
  ) {
    function wrapped(...args: Parameters<T>): void {
      const recipe = callback(...args);
      const nextValue = produce(state, recipe);
      setState(nextValue);
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
    return useCallback(wrapped, [state, setState, ...deps]);
  };
}

/**
 * The useDraftableState is a wrapper for the useState hook provided by React.
 * It provides a third value `produceValue` which takes a callback for producing the next state.
 *
 * This is a convenience wrapper for the useDraft hook which will internally use the useState hook
 * and return its values.
 *
 * For more information, see the documentation for immer.js's produce
 * https://immerjs.github.io/immer/produce
 * @param initialState the initial state for the hook
 * @returns the state, the setState callback and a produce hook for altering the state via an immer.js draft callback.
 *
 * @example
 *  const [user, setUser, produceUser] = useDraftableState<{id: string, name: string}>({id: '', name: ''});
 *  const setUserName = useCallback((nextName: string) => produceUser(user => user.name = nextName, []));
 */
export function useDraftableState<S>(
  initialState: S,
): [
  S,
  Dispatch<SetStateAction<S>>,
  (producer: (draft: Draft<S>) => void, deps: readonly any[]) => void,
  <T extends DraftCallback<S>>(
    callback: T,
    deps: readonly any[],
  ) => (...args: Parameters<T>) => void,
] {
  const [value, setValue] = useState<S>(initialState);
  const produceValue = useDraft(value, setValue);
  const produceCallback = useDraftCallback(value, setValue);
  return [value, setValue, produceValue, produceCallback];
}
