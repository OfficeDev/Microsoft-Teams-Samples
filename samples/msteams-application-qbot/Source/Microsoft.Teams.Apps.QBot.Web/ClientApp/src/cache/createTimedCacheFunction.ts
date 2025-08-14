interface CacheEntry<T> {
  lastUpdated: number;
  entry: T;
}

/**
 * Create a function that only re-computes the return value if/when it is expired.
 *
 * Note: This function sets up timers to clear the 'stale' values around the time they expire.
 * There will be about as many active timers as currently cached values.
 * If you have a high-cardinality & high frequency function this can cause memory and performance issues
 * as these timers all execute.
 *
 * @param expireMs how many milliseconds to wait before evicting the value from cache.
 * @param keyFn the function to calculate the cache key
 * @param fn the funciton to decorate
 * @returns a promise for the result.
 */
export function createTimedCacheFunction<
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  T extends (...args: any[]) => any,
  TCacheKey
>(
  expireMs: number,
  keyFn: (...args: Parameters<T>) => TCacheKey,
  fn: T,
): (...args: Parameters<T>) => ReturnType<T> {
  // The cached values
  const cache = new Map<TCacheKey, CacheEntry<ReturnType<T>>>();
  // The timers for clearing the cached values
  const evictionTimers = new Map<TCacheKey, NodeJS.Timeout>();

  // Start the timer for clearing the cached value
  // This is used to ensure that stale entries don't
  // hang around in memory unnecessarily
  function startCacheClearTimer(key: TCacheKey) {
    if (evictionTimers.has(key)) {
      const oldTimer = evictionTimers.get(key) as NodeJS.Timeout;
      clearTimeout(oldTimer);
    }
    const timer = setTimeout(function () {
      const { lastUpdated } = cache.get(key) as CacheEntry<ReturnType<T>>;
      if (Date.now() < lastUpdated + expireMs) {
        return setImmediate(() => startCacheClearTimer(key));
      }
      cache.delete(key);
      evictionTimers.delete(key);
    }, expireMs);
    evictionTimers.set(key, timer);
  }

  // The wrapper function for the decorated function
  return function (...args: Parameters<T>) {
    // calculate the cache key
    const key = keyFn(...args);

    // Grab when the entry was inserted / last updated in ms since epoch
    // the default for non-existant entries is zero so the new value is written.
    const { lastUpdated } = (cache.get(key) as CacheEntry<ReturnType<T>>) ?? {
      lastUpdated: 0, // we won't be dealing with pre-epoch cache values - zero works as a default
    };

    // Grab the current time in ms since epoch
    const now = Date.now();
    // if the value is expired, recompute & store in cache.
    if (now >= lastUpdated + expireMs) {
      cache.set(key, {
        lastUpdated: Date.now(),
        entry: fn(...args),
      });

      // Start a timer to clear the cache key.
      startCacheClearTimer(key);
    }

    // Since access is (relatively) cheap, just access the value out of cache for all paths &
    // return
    const { entry } = cache.get(key) as CacheEntry<ReturnType<T>>;
    return entry;
  };
}
