/* eslint-disable @typescript-eslint/no-explicit-any */
import {
  useMemo,
  useState,
  Dispatch,
  SetStateAction,
  useCallback,
} from 'react';
import { castDraft } from 'immer';
import { useDraftableState } from './useDraftableState';

const FilterFuncDepsSymbol = Symbol.for('FilterFuncDeps');

/**
 * A Filter is a callback for filtering a list that also carries a list of its depedencies with it.
 * This is so that we can efficiently filter arrays once but also maintain the convenience of declaring
 * filters separately.
 */
export type Filter<TItem> = { [FilterFuncDepsSymbol]: readonly any[] } & ((
  item: TItem,
) => boolean);

/**
 * The useFilter is the simpliest filter creation hook that converts a filter function & its dependencies into
 * a Filter object.
 * @param filter The filter callback
 * @param deps The filter's dependencies
 * @returns the filter
 */
export function useFilter<TItem>(
  filter: (item: TItem) => boolean,
  deps: readonly any[],
): Filter<TItem> {
  const decoratedFilter = filter as Filter<TItem>;
  decoratedFilter[FilterFuncDepsSymbol] = deps;
  return decoratedFilter;
}

/**
 * The useFilterableList hook filters a list with one or more Filters. It memoizes the result based on the
 * dependencies declared in the creation of the individual filters.
 * @param items The input array
 * @param filters the filters
 * @returns the filtered array
 */
export function useFilterableList<TItem>(
  items: readonly TItem[],
  ...filters: Filter<TItem>[]
): TItem[] {
  const deps = filters.flatMap((func) => func[FilterFuncDepsSymbol]);
  return useMemo(
    () => items.filter((item) => filters.every((filter) => filter(item))),
    // eslint-disable-next-line react-hooks/exhaustive-deps
    [items, ...deps],
  );
}

/**
 * The useStatefulFilter declares state that will be used to filter the list.
 * This is a shorthand for combining the useState & the useFilter hooks.
 * @param testFn a function for testing if the current state filters an individual element
 * @param initialState the initial value of the state
 * @returns the state & setState from useState and the filter derived from the state.
 */
export function useStatefulFilter<TItem, TValue>(
  testFn: (item: TItem, value: TValue) => boolean,
  initialState: TValue,
): {
  state: TValue;
  setState: Dispatch<SetStateAction<TValue>>;
  filter: Filter<TItem>;
} {
  const [state, setState] = useState<TValue>(initialState);
  const filter = useFilter((item: TItem) => testFn(item, state), [state]);
  return { state, setState, filter };
}

/**
 * The useOptionalStatefulFilter declares state that will be used to filter the list with a sentinal 'undefined' value that
 * skips filtering. This is useful for optional filters such as those found in view-level filters where the empty state of the filter
 * returns all elements instead of none.
 * @param testFn a function for testing if the current state filters an individual element
 * @param initialState the optional initial value of the state
 * @returns the state & setState from useState and the filter derived from the state.
 */
export function useOptionalStatefulFilter<TItem, TValue>(
  testFn: (item: TItem, value: TValue) => boolean,
  initialState: TValue | undefined = undefined,
): {
  state: TValue | undefined;
  setState: Dispatch<SetStateAction<TValue | undefined>>;
  clearState: () => void;
  filter: Filter<TItem>;
} {
  const { state, setState, filter } = useStatefulFilter(
    (item: TItem, value: TValue | undefined) =>
      value === undefined ? true : testFn(item, value),
    initialState,
  );
  const clearValue = useCallback(() => setState(undefined), [setState]);
  return { state, setState, filter, clearState: clearValue };
}

/**
 * The useEqualityFilter is a shorthand for a stateful filter that checks optional equality on a property of
 * an item in the list. This is useful for when you have a picker element such as a radio button
 * that selects one value that is then filtering the entire list.
 *
 * @param propertyFn a function that gets the property off an item.
 * @param initialState the initial value of the state
 * @returns the state & setState from useState and the filter derived from the state
 */
export function useEqualityFilter<TItem, TValue>(
  propertyFn: (item: TItem) => TValue,
  initialState: TValue | undefined = undefined,
): {
  state: TValue | undefined;
  setState: Dispatch<SetStateAction<TValue | undefined>>;
  filter: Filter<TItem>;
} {
  return useOptionalStatefulFilter(
    (item: TItem, value: TValue) => propertyFn(item) === value,
    initialState,
  );
}

/**
 * The useSetFilter is a shorthand for a stateful filter that picks elements whose property is a member of a
 * set. This is useful for multi-select inputs where elements need to be a one-of relationship to the filter.
 * @param propertyFn  a function that gets the property off an item.
 * @returns the state & setState from useState and the filter derived from the state
 */
export function useSetFilter<TItem, TValue>(
  propertyFn: (item: TItem) => TValue,
): {
  value: Set<TValue>;
  setValue: Dispatch<SetStateAction<Set<TValue>>>;
  filter: Filter<TItem>;
  clearValues: () => void;
  addValue: (item: TValue) => void;
  removeValue: (item: TValue) => void;
  toggleValue: (item: TValue) => void;
} {
  const [value, setValue, , produceCallback] = useDraftableState(
    new Set<TValue>(),
  );
  const filter = useFilter(
    (item: TItem) => (value.size === 0 ? true : value.has(propertyFn(item))),
    [value],
  );
  const clearValues = produceCallback(
    () => (nextValue) => nextValue.clear(),
    [],
  );
  const addValue = produceCallback(
    (item: TValue) => (nextValue) => nextValue.add(castDraft(item)),
    [],
  );
  const removeValue = produceCallback(
    (item: TValue) => (nextValue) => nextValue.delete(castDraft(item)),
    [],
  );
  const toggleValue = produceCallback(
    (item: TValue) => (nextValue) =>
      value.has(item)
        ? nextValue.delete(castDraft(item))
        : nextValue.add(castDraft(item)),
    [],
  );
  return {
    value,
    setValue,
    filter,
    clearValues,
    addValue,
    removeValue,
    toggleValue,
  };
}

export function useRangeFilter<TItem>(
  propertyFn: (item: TItem) => number,
  initialMin?: number | undefined,
  initialMax?: number | undefined,
): {
  filter: Filter<TItem>;
  minValue: number;
  maxValue: number;
  setMinValue: (min: number) => void;
  setMaxValue: (max: number) => void;
} {
  const [minValue, setMinValue] = useState<number>(
    initialMin ?? Number.MIN_VALUE,
  );
  const [maxValue, setMaxValue] = useState<number>(
    initialMax ?? Number.MAX_VALUE,
  );
  // TODO(nibeauli): add an effect that warns if min >= max!
  const filter = useFilter(
    (item: TItem) => {
      const itemValue = propertyFn(item);
      return itemValue > minValue && itemValue <= maxValue;
    },
    [minValue, maxValue],
  );
  return { filter, minValue, maxValue, setMinValue, setMaxValue };
}

export function useSubstringFilter<TItem>(
  propertyFn: (item: TItem) => string[],
  initialQuery?: string | undefined,
  { caseInsensitive }: { caseInsensitive: boolean } = {
    caseInsensitive: false,
  },
): {
  filter: Filter<TItem>;
  query: string | undefined;
  setQuery: Dispatch<SetStateAction<string | undefined>>;
} {
  const { filter, state: query, setState: setQuery } = useStatefulFilter(
    (item: TItem, query: string | undefined) => {
      if (query === undefined || query === '') return true;
      const itemValues = propertyFn(item);

      const queryValue = caseInsensitive ? query.toUpperCase() : query;
      return caseInsensitive
        ? itemValues.some((str) => str.toUpperCase().indexOf(queryValue) >= 0)
        : itemValues.some((str) => str.indexOf(queryValue) >= 0);
    },
    initialQuery,
  );
  return { filter, query, setQuery };
}
