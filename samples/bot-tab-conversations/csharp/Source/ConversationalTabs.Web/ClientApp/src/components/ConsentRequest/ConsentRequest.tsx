import * as microsoftTeams from '@microsoft/teams-js';
import { Button, Flex, Header, Text } from '@fluentui/react-northstar';

type ConsentRequestProps = {
  callback: (error?: string, result?: string) => void;
};

function ConsentRequest({ callback }: ConsentRequestProps) {
  const callConsentAuth = () => {
    microsoftTeams.authentication.authenticate({
      url: `${window.location.origin}/auth-start`,
      width: 600,
      height: 535,
      successCallback: async (result) => {
        console.log('Consent provided.');
        callback(undefined, result);
      },
      failureCallback: (error) => {
        console.error("Failed to get consent: '" + error + "'");
        callback(error);
      },
    });
  }

  return (
    <Flex column>
      <Header content="To complete that action you must consent" />
      <Text as="p" content="That action includes a call to an API that we do not have permission to call. To complete it you must allow us to access some details from you Microsoft account." />
      <Text as="p" content="Clicking 'Consent' will open a dialog window, if you do not see it, please check your pop-up blocker." />
      <Button primary content="Consent" onClick={callConsentAuth}/>
    </Flex>
  );
}

export { ConsentRequest };
