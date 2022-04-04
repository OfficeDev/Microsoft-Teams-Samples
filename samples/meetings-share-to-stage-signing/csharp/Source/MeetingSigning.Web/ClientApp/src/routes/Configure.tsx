import { useEffect } from 'react';
import * as microsoftTeams from '@microsoft/teams-js';
import { Header, Text } from '@fluentui/react-northstar';

/**
 * Component rendered when configuring the Tab
 */
export default function Configure() {
  useEffect(() => {
    microsoftTeams.settings.registerOnSaveHandler((saveEvent) => {
      microsoftTeams.settings.setSettings({
        contentUrl: window.location.origin,
        entityId: window.location.origin,
      });

      saveEvent.notifySuccess();
    });
    microsoftTeams.settings.setValidityState(true);
  });

  return (
    <>
      <Header as="h1" content="Thanks for installing Meeting Signing." />
      <Text as="p" content="To complete installation, click Save below." />
    </>
  );
}
