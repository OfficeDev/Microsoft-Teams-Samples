// eslint-disable-next-line @typescript-eslint/no-explicit-any
type AsyncFunction = (...args: any[]) => Promise<any>;
type AsyncFunctionReturnType<T extends AsyncFunction> = T extends (
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  ...args: any[]
) => Promise<infer R>
  ? R
  : never;

/**
 * Decorate an async function so that it will only have one execution in flight per
 * argument set.
 *
 * This can be used to ensure concurrent requests for the same 'expensive' operation are
 * merged together by waiting for the same Promise rather than executing the expensive operation.
 *
 * An example would be wrapping an API call so that concurrent requests for the same data results in only
 * one round trip, but each caller would get the same data
 * @param keyFn function for mapping the arguments to a comparable key
 * @param decoratedFn the decorated async function to make single entrant by argument set
 * @returns a function that is single entrant by argument set
 */
export function createSingleEntrantFunction<T extends AsyncFunction, TCacheKey>(
  keyFn: (...args: Parameters<T>) => TCacheKey,
  decoratedFn: T,
): (...args: Parameters<T>) => Promise<AsyncFunctionReturnType<T>> {
  // Map of key => currently pending promise
  const inFlightMap = new Map<TCacheKey, Promise<AsyncFunctionReturnType<T>>>();
  // The replacement function that figures out the single-entrancy
  return function (
    ...args: Parameters<T>
  ): Promise<AsyncFunctionReturnType<T>> {
    // From the arguments calculate the comparable key
    const key = keyFn(...args);
    // check to see if there is already a pending request for those arguments
    if (!inFlightMap.has(key)) {
      // there was no pending request, go ahead and execute it
      const promise = decoratedFn(...args);
      // when the request is done, we need to remove it from the mapping
      // so that subsequent calls will go through
      promise.finally(() => {
        inFlightMap.delete(key);
      });
      // set the key in the map so we de-duplicate future invocations
      inFlightMap.set(key, promise);
    }
    return inFlightMap.get(key) as Promise<AsyncFunctionReturnType<T>>;
  };
}
