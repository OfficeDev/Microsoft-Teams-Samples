import { useContext, useState } from 'react';
import * as microsoftTeams from '@microsoft/teams-js';
import storageAvailable from 'storage-available';
import { Alert, Button, Flex, Header, Text } from '@fluentui/react-northstar';
import { TeamsContext } from 'utils/TeamsProvider/TeamsProvider';

export function AnonymousPage() {
  const [error, setError] = useState<string | undefined>(undefined);
  const { setAnonymousUserAccessToken } = useContext(TeamsContext);

  const authenticateMSA = async () => {
    try {
      const token = await microsoftTeams.authentication.authenticate({
        url: `${window.location.origin}/auth-start/msa`,
        width: 600,
        height: 535,
      });

      setAnonymousUserAccessToken(token);

      // If we can't store the token in local storage, we may be in incognito
      // We need to pass the token in memory instead of relying on MSAL.
      if (!storageAvailable('localStorage')) {
        globalThis.anonymousUserAccessToken = token;
      }
    } catch (e: any) {
      setError(e.message);
    }
  };

  return (
    <>
      <Flex column>
        <Header
          as="h1"
          content="You need to verify your identity before you can continue."
        />
        <Text
          as="p"
          content="Currently only personal Microsoft accounts are supported."
        />
        {!storageAvailable('localStorage') && (
          <Text
            as="p"
            temporary
            content="You may have to sign in multiple times as it appears you are using a private window or a browser that doesn't support storing your login details"
          />
        )}
        <Button onClick={authenticateMSA}>Sign in with Microsoft</Button>
        {error && <Alert header="Error" content={error} danger visible />}
      </Flex>
    </>
  );
}
