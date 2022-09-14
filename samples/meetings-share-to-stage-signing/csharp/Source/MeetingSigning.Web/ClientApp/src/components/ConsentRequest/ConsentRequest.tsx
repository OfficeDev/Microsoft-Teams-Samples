import * as microsoftTeams from '@microsoft/teams-js';
import { Button, Flex, Header, Text } from '@fluentui/react-northstar';

type ConsentRequestProps = {
  callback: (error?: string, result?: string) => void;
};

function ConsentRequest({ callback }: ConsentRequestProps) {
  const callConsentAuth = async () => {
    try {
      const result = await microsoftTeams.authentication.authenticate({
        url: `${window.location.origin}/auth-start/aad`,
        width: 600,
        height: 535,
      });

      console.log('Consent provided.');
      callback(undefined, result);
    } catch (error: any) {
      console.error(`Failed to get consent: "${error}"`);
      callback(error);
    }
  };

  return (
    <Flex column>
      <Header as="h2" content="To complete that action you must consent" />
      <Text
        as="p"
        content="We need your permission to access some data from your account."
      />
      <Button primary content="Consent" onClick={callConsentAuth} />
    </Flex>
  );
}

export { ConsentRequest };
