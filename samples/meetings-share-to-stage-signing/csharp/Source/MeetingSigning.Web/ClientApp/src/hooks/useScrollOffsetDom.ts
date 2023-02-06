import { useCallback, useEffect, useState } from 'react';

export interface ScrollOffset {
  /**
   * The percentage of the document scrolled horizontally.
   */
  scrollXPercentage: number;

  /**
   * The percentage of the document scrolled vertically.
   */
  scrollYPercentage: number;
}
/**
 * A hook that handles the scroll offset events in the DOM, for a given div element.
 *
 * @remarks We have split the scroll offset object into two parts: one focused
 * on the DOM and one focuses on Live Share. This helps to separate the concerns.
 *
 * @param divElement Reference to the DOM div element to listen to
 *
 * @returns an object containing:
 * - position: Current scroll position as a percentage of the element
 * - setPosition: Callback to set the scroll position in the DOM
 */
export function useScrollOffsetDom(divElement: HTMLDivElement | undefined | null) {
  var [position, setPosition] = useState<ScrollOffset>({
    scrollXPercentage: 0,
    scrollYPercentage: 0,
  });

  // Save the div elements scroll position in state
  var callback = useCallback(() => {
    if (divElement) {
      setPosition({
        scrollXPercentage: window.scrollX / divElement.offsetWidth,
        scrollYPercentage: window.scrollY / divElement.offsetHeight,
      });
    }
  }, [divElement, setPosition]);

  useEffect(() => {
    // Add event listeners to save the scroll offset position into state when the document is scrolled.
    window.addEventListener('scroll', callback);
    return () => window.removeEventListener('scroll', callback);
  }, [callback]);

  // Scroll the div element to a specific div
  var externalCallback = useCallback(
    (position: ScrollOffset) => {
      if (divElement === undefined || divElement === null) {
        return;
      }

      setPosition(position);
      window.scrollTo(
        divElement.offsetWidth * position.scrollXPercentage,
        divElement.offsetHeight * position.scrollYPercentage,
      );
    },
    [divElement],
  );

  return {
    position,
    setPosition: externalCallback,
  };
}
