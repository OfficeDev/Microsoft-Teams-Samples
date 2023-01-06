import { useCallback, useEffect, useState } from 'react';

export interface CursorLocation {
  /**
   * The X coordinate position
   */
  X: number;

  /**
   * The Y coordinate position
   */
  Y: number;
}

/**
 * A hook that handles the cursor location events in the DOM, for a given div element.
 *
 * @param divElement Reference to the DOM div element to listen to.
 * @returns an object containing:
 * - cursorlocation: The X, Y location of the cursor's position
 */
export function useCursorLocationDom(divElement: HTMLDivElement | undefined | null) {
  var [cursorLocation, setCursorLocation] = useState<CursorLocation>({
    X: 0,
    Y: 0,
  });

  var callback = useCallback((event: MouseEvent) => {
    if (divElement) {
      const divBoundingClient = divElement.getBoundingClientRect();
      // Mouse Events contain multiple coordinates. We use pageX/pageY and subtract the bounding boxes position
      setCursorLocation({
        X: event.pageX - divBoundingClient.left,
        Y: event.pageY - divBoundingClient.top,
      });
    }
  }, [divElement]);

  useEffect(() => {
    if (divElement) {
      divElement.addEventListener('mousemove', callback);
      return () => divElement.removeEventListener('mousemove', callback);
    }
  }, [divElement, callback]);

  return {
    cursorLocation,
  };
}
