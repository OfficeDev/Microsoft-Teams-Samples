// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
import React, { useEffect, useState } from 'react';
import { Text, Input, Flex } from '@fluentui/react-northstar';
import * as microsoftTeams from '@microsoft/teams-js';
import { TeamsService } from 'services/TeamsService';
import { useDefaultColorScheme } from 'hooks/useColorScheme';
import { FormattedMessage } from 'react-intl';

export default function ConfigTab(): JSX.Element {
  const [displayName, setDisplayName] = useState('Dashboard');
  const colorScheme = useDefaultColorScheme();

  useEffect(() => {
    microsoftTeams.initialize();
    microsoftTeams.settings.registerOnSaveHandler(async (saveEvent) => {
      await new TeamsService().getContext().then((context) => {
        console.log('context', context);
        microsoftTeams.settings.setSettings({
          entityId: '',
          contentUrl: `https://${window.location.host}/share/courses/${context.groupId}/channel/${context.channelId}`,
          suggestedDisplayName: displayName,
        });
        saveEvent.notifySuccess();
      });
    });
    microsoftTeams.settings.setValidityState(true);
  });

  return (
    <Flex column gap="gap.smaller">
      <Text
        style={{ color: colorScheme.foreground2 }}
        content={
          <FormattedMessage
            id="configTab.tabName"
            description="Name of the shared tab"
            defaultMessage="Name"
          />
        }
        size="small"
      />
      <Input
        onChange={(e) => {
          const target = e.target as HTMLInputElement;
          setDisplayName(target?.value);
        }}
        as="div"
        maxLength={100}
        fluid
        value={displayName}
        autoComplete="off"
      />
    </Flex>
  );
}
