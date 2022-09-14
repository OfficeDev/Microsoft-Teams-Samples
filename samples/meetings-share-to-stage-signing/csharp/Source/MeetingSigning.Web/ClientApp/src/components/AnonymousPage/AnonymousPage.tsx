import { useContext, useState } from 'react';
import * as microsoftTeams from '@microsoft/teams-js';
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
        <Text content="Currently only personal Microsoft accounts are supported." />
        <Button onClick={authenticateMSA}>Sign in with Microsoft</Button>
        {error && <Alert header="Error" content={error} danger visible />}
      </Flex>
    </>
  );
}
