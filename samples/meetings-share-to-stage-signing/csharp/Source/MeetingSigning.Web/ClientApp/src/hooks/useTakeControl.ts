import { useCallback, useEffect, useState, useRef } from 'react';
import { app } from '@microsoft/teams-js';
import {
  LiveEvent,
  LiveState,
  UserMeetingRole,
} from '@microsoft/live-share';
import { IAzureAudience } from '@fluidframework/azure-client';
import { throttle } from 'lodash';

const ROLES_ALLOWED_TO_TAKE_CONTROL = [
  UserMeetingRole.organizer,
  UserMeetingRole.presenter,
];

export interface UserInControl {
  userId?: string;
  displayName?: string;
}

/**
 * A hook that handles the take control functionality
 *
 * @param takeControlState Live Share state that handles the taking of control
 * @param userInfo The info of the local user
 * @param audience Audience from the Fluid Client services
 * @returns An object containing:
 * - `takeControlStarted` - Boolean that indicates if the take control state has started
 * - `localUserInControl` - Is the current user currently in control
 * - `localUserCanTakeControl` - Is the current user allowed to take control.
 * - `takeControl` - Callback to allow a user to take control of the presentation
 */
export const useTakeControl = (
  takeControlState: LiveState<UserInControl>,
  userInfo?: app.UserInfo,
  audience?: IAzureAudience,
) => {
  const [takeControlStarted, setTakeControlStarted] = useState(false);
  const [localUserCanTakeControl, setLocalUserCanTakeControl] = useState(false);
  const controlCleared = useRef<boolean>(false);

  const sendLatestTakeControl = useCallback(() => {
    takeControlState?.changeState('userInControl', {
      // Using context to get the user's information is not guaranteed to be accurate, as the
      // local user could alter it before the state is updated. Today, there is no way in Live Share to
      // prove who sent the update, so this is relying on meeting participants being good citizens.
      userId: controlCleared.current ? undefined : userInfo?.id ?? '',
      displayName: controlCleared.current
        ? undefined
        : userInfo?.userPrincipalName ?? '',
    });
  }, [takeControlState, userInfo]);

  const throttledSendLatestTakeControl = useCallback(
    // Throttle the sending of the take control to once per 500 milliseconds
    // to prevent sending too many events to the Azure Fluid Relay
    throttle(sendLatestTakeControl, 500),
    [sendLatestTakeControl],
  );

  const takeOrClearControl = useCallback(
    (takeControl: boolean) => {
      // As we throttle the sending of events, we only need to save the latest position.
      controlCleared.current = !takeControl;
      throttledSendLatestTakeControl();
    },
    [takeControlState, throttledSendLatestTakeControl],
  );

  useEffect(() => {
    const interval = setInterval(async () => {
      try {
        // The below is how we can get a user's roles in a meeting.
        // It is used to change the UI based on the user's roles

        // First we get the user's clientId from the audience.
        // Note this value will change if the client reconnects to the container,
        // and a user can have multiple clientIds, one for each connection.
        let currentUserClientId = audience?.getMyself()?.connections[0]?.id;

        if (currentUserClientId === undefined) {
          return false;
        }

        // Next we call getClientRoles to get the user's roles based on their clientId.
        // This call is cached for up to 5 minutes
        let currentUserRoles = await LiveEvent.getClientRoles(
          currentUserClientId,
        );

        // Then we check if the current user has any of the allowed roles.
        setLocalUserCanTakeControl(
          ROLES_ALLOWED_TO_TAKE_CONTROL.filter((role) =>
            currentUserRoles.includes(role),
          ).length > 0,
        );
      } catch (error) {
        console.log(error);
        setLocalUserCanTakeControl(false);
      }
    }, 15000);
    return () => clearInterval(interval);
  }, [audience]);

  useEffect(() => {
    if (takeControlState && !takeControlState.isInitialized) {
      takeControlState
        .initialize(ROLES_ALLOWED_TO_TAKE_CONTROL)
        .then(() => {
          setTakeControlStarted(true);
        })
        .catch((error) => {
          console.error(error);
        });
    }
  }, [takeControlState]);

  return {
    takeControlStarted,
    localUserInControl: takeControlState?.data?.userId === userInfo?.id,
    localUserCanTakeControl,
    takeControl: () => takeOrClearControl(true),
    clearControl: () => takeOrClearControl(false),
  };
};
