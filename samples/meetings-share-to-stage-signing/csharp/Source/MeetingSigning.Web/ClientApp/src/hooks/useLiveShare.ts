import { useEffect, useState } from 'react';
import {
  EphemeralEvent,
  EphemeralPresence,
  EphemeralState,
  TeamsFluidClient,
} from '@microsoft/live-share';
import { IFluidContainer } from 'fluid-framework';
import { IAzureAudience } from '@fluidframework/azure-client';
import { ScrollOffsetEvent } from './useScrollOffsetLiveShare';
import { CursorLocationEvent } from './useCursorLocationsLiveShare';
import { UserInControl } from './useTakeControl';

type LiveShareContainerSchema = {
  scrollOffsetEvent: EphemeralEvent<ScrollOffsetEvent>;
  cursorLocationsEvent: EphemeralPresence<CursorLocationEvent>;
  takeControlState: EphemeralState<UserInControl>;
};

/**
 * Hook that creates/loads the apps shared objects.
 *
 * @remarks
 * This is an application specific hook that defines the fluid schema of Distributed Data Structures (DDS)
 * used by the app and passes that schema to the `TeamsFluidClient` to create/load your Fluid container.
 *
 * @returns Shared objects managed by the apps fluid container.
 */
export function useLiveShare() {
  const [results, setResults] = useState<{
    container: IFluidContainer;
    audience: IAzureAudience;
  }>();
  const [error, setError] = useState();

  useEffect(() => {
    // Define container schema
    const schema = {
      initialObjects: {
        scrollOffsetEvent: EphemeralEvent,
        cursorLocationsEvent: EphemeralPresence,
        takeControlState: EphemeralState,
      },
    };

    // Join Teams container
    const client = new TeamsFluidClient();
    client
      .joinContainer(schema)
      .then(({ container, services }) => {
        setResults({ container, audience: services?.audience });
      })
      .catch((err) => {
        setError(err);
      });
  }, []);

  return {
    ...(results?.container?.initialObjects as LiveShareContainerSchema),
    ...results,
    error,
  };
}
