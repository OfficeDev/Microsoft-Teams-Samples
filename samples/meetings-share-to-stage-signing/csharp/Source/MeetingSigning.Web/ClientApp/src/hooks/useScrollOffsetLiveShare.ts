import { useCallback, useEffect, useState, useRef } from 'react';
import {
  LiveEvent,
  LiveState,
  ILiveEvent,
  UserMeetingRole,
} from '@microsoft/live-share';
import { throttle } from 'lodash';
import { ScrollOffset } from './useScrollOffsetDom';
import { UserInControl } from 'hooks';

export interface ScrollOffsetEvent extends ScrollOffset, ILiveEvent {
  userId?: string;
}

/**
 * A hook that handles the scroll offset events when related to Live Share.
 *
 * @remarks We have split the scroll offset object into two parts: one focused
 * on the DOM and one focuses on Live Share. This helps to separate the concerns.
 *
 * @param scrollOffsetEvent Live Share event that handles firing on a scroll
 * @param setPosition Callback to set the scroll position in the DOM
 * @param takeControlState Live share state on whether the user is in control
 * @param localUserInControl Is the current user in control of scrolling
 * @param localUserId The current user's ID
 * @returns An object containing:
 * - `scrollOffsetStarted`: Boolean that indicates if the scroll offset event has started
 * - `followSuspended`: Boolean that indicates if the current user is not following the user in control
 * - `endSuspension`: A callback function that ends the suspension, and follows the user in control again
 * - `sendScrollOffset`: A callback function that will send the scroll offset to the Live Share event
 */
export const useScrollOffsetLiveShare = (
  scrollOffsetEvent: LiveEvent<ScrollOffsetEvent>,
  setPosition: (position: ScrollOffset) => void,
  takeControlState: LiveState<UserInControl>,
  localUserInControl: boolean,
  localUserId?: string,
) => {
  const [scrollOffsetStarted, setScrollOffsetStarted] = useState(false);
  const latestPosition = useRef<ScrollOffset | undefined>(undefined);
  const latestLiveSharePosition = useRef<ScrollOffset | undefined>(undefined);
  const suspended = useRef<boolean>(false);

  const sendLatestScrollOffset = useCallback(() => {
    if (latestPosition.current !== undefined && localUserInControl) {
      scrollOffsetEvent?.sendEvent({
        ...latestPosition.current,
        userId: localUserId,
        timestamp: LiveEvent.getTimestamp(),
      });

      // We set the scroll offset to undefined to prevent resending the same data multiple times.
      latestPosition.current = undefined;
      suspended.current = false;
    } else if (
      // We check to see if the user has scrolled more than a small percent from the user in control,
      // before we decide if we should suspend following. If we did an exact match we might end up suspending
      // unnecessarily, especially when multiple events come in
      (Math.abs(
        (latestPosition.current?.scrollXPercentage ?? 0) -
          (latestLiveSharePosition?.current?.scrollXPercentage ?? 0),
      ) > 0.025 ||
        Math.abs(
          (latestPosition.current?.scrollYPercentage ?? 0) -
            (latestLiveSharePosition?.current?.scrollYPercentage ?? 0),
        ) > 0.025) &&
      !localUserInControl
    ) {
      suspended.current = true;
    }
  }, [scrollOffsetEvent, localUserInControl, localUserId, latestPosition]);

  const throttledSendLatestScrollOffset = useCallback(
    // Throttle the sending of the scroll offset to once per 50 milliseconds
    // to prevent sending too many events to the Azure Fluid Relay
    throttle(sendLatestScrollOffset, 50),
    [sendLatestScrollOffset],
  );

  const sendScrollOffset = useCallback(
    (value: ScrollOffset) => {
      // As we throttle the sending of events, we only need to save the latest position.
      latestPosition.current = value;
      throttledSendLatestScrollOffset();
    },
    [scrollOffsetEvent, throttledSendLatestScrollOffset],
  );

  const endSuspension = useCallback(() => {
    suspended.current = false;
    latestPosition.current = undefined;
    latestLiveSharePosition.current &&
      setPosition(latestLiveSharePosition.current);
  }, [latestLiveSharePosition, setPosition]);

  useEffect(() => {
    if (!scrollOffsetEvent || scrollOffsetEvent.isInitialized) {
      return;
    }

    scrollOffsetEvent.on(
      'received',
      (value: ScrollOffsetEvent, local: boolean) => {
        if (
          local ||
          !value?.userId ||
          value.userId !== takeControlState.data?.userId
        ) {
          // Ignore the event if it's not from the user in control
          // Or are from the current user
          return;
        }

        // Only scroll if the viewer is not suspended
        if (!suspended.current) {
          setPosition(value);
        }

        // But save the in control user's position either way, so that if we unsuspend
        // we can move to the controller's location
        latestLiveSharePosition.current = value;
      },
    );

    scrollOffsetEvent
      .initialize([UserMeetingRole.organizer, UserMeetingRole.presenter])
      .then(() => {
        setScrollOffsetStarted(true);
      })
      .catch((error) => {
        console.error(error);
      });
  }, [scrollOffsetEvent, takeControlState, setPosition]);

  return {
    scrollOffsetStarted,
    followSuspended: suspended.current,
    endSuspension,
    sendScrollOffset,
  };
};
