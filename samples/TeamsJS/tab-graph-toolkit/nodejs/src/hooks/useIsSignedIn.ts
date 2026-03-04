import { useState, useEffect } from 'react';
import { Providers, ProvidersChangedState, ProviderState } from '@microsoft/mgt-element';

export function useIsSignedIn(): [boolean] {
  const [isSignedIn, setIsSignedIn] = useState(false);

  useEffect(() => {
    const updateState = (stateEvent: ProvidersChangedState) => {
      if (stateEvent === ProvidersChangedState.ProviderStateChanged) {
        setIsSignedIn(Providers.globalProvider.state === ProviderState.SignedIn);
      }
    };

    setIsSignedIn(Providers.globalProvider.state === ProviderState.SignedIn);
    Providers.onProviderUpdated(updateState);

    return () => {
      Providers.removeProviderUpdatedListener(updateState);
    };
  }, []);

  return [isSignedIn];
}
