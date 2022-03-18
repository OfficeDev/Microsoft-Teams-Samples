import { useEffect, useRef } from 'react';

/**
 * Creates a React Hook that sets interval to poll a function.
 *
 * @param callback The function to be called.
 * @param delay Time interval between API calls in milli seconds.
 *
 */
export function useInterval(callback: () => void, delay: number) {
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  const savedCallback = useRef<any | null>();

  // Remember the latest callback.
  useEffect(() => {
    savedCallback.current = callback;
  }, [callback]);

  // Set up the interval.
  useEffect(() => {
    function tick() {
      savedCallback.current();
    }
    if (delay !== null) {
      const id = setInterval(tick, delay);
      return () => clearInterval(id);
    }
  }, [delay]);
}
