import { useCallback, useEffect, useRef, useState } from 'react';
import { LivePresence, PresenceState } from '@microsoft/live-share';
import { throttle } from 'lodash';
import { CursorLocation } from './useCursorLocationDom';

export interface CursorLocationEvent extends CursorLocation {
  displayName?: string;
}

/**
 * A hook that handles cursor location events when related to Live Share.
 *
 * @param cursorLocationsEvent Live Share event that handles firing on a cursor location
 * @param userDisplayName The display name of the user
 * @param userId The userId that is used as the Presence userId
 * @returns Object containing:
 * - `cursorLocationsStarted` Boolean that indicates if the cursor location event has started
 * - `sendCursorLocation` A callback function that will send the cursor location to the Live Share event
 */
export const useCursorLocationsLiveShare = (
  cursorLocationsEvent: LivePresence<CursorLocationEvent>,
  userDisplayName: string | undefined,
  userId?: string,
) => {
  const [cursorLocationsStarted, setCursorLocationsStarted] =
    useState<boolean>(false);
  const latestCursorLocation = useRef<CursorLocation | undefined>(undefined);

  const sendLatestCursorLocations = useCallback(() => {
    if (latestCursorLocation.current !== undefined && cursorLocationsEvent?.isInitialized) {
      // Using context to get the user's information is not guaranteed to be accurate, as the
      // local user could alter it before the state is updated. Today, there is no way in Live Share to
      // prove who sent the update, so this is relying on meeting participants being good citizens.
      cursorLocationsEvent?.updatePresence(PresenceState.online, {
        ...latestCursorLocation.current,
        displayName: userDisplayName,
      });
      latestCursorLocation.current = undefined;
    }
  }, [cursorLocationsEvent, userDisplayName, latestCursorLocation]);

  const throttledSendLatestCursorLocations = useCallback(
    // Throttle the sending of the scroll offset to once per 50 milliseconds
    // to prevent sending too many events to the Azure Fluid Relay
    throttle(sendLatestCursorLocations, 50),
    [sendLatestCursorLocations],
  );

  const sendCursorLocation = useCallback(
    (value: CursorLocation) => {
      latestCursorLocation.current = value;
      throttledSendLatestCursorLocations();
    },
    [throttledSendLatestCursorLocations],
  );

  useEffect(() => {
    if (cursorLocationsEvent && !cursorLocationsEvent.isInitialized) {
      cursorLocationsEvent.expirationPeriod = 5;
      cursorLocationsEvent
        .initialize(userId)
        .then(() => {
          setCursorLocationsStarted(true);
        })
        .catch((error) => console.error(error));
    }
  }, [cursorLocationsEvent, userId]);

  return {
    cursorLocationsStarted,
    sendCursorLocation,
  };
};
