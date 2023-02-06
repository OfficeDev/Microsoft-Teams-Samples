import { Button, Flex, Text } from '@fluentui/react-northstar';
import React from 'react';
import * as microsoftTeams from '@microsoft/teams-js';
import { FormattedMessage } from 'react-intl';
import SearchKeyPresent from './SearchKey';
import NoSearchKey from './NoSearchKey';
import { useConfig } from 'hooks/useConfig';

// eslint-disable-next-line sonarjs/cognitive-complexity
function NoQuestions({
  selectedCourseId,
  selectedTab,
  selectedCourseUser,
  channelId,
  searchKey,
}: {
  selectedCourseId: string;
  selectedTab: number;
  selectedCourseUser: string | undefined;
  channelId: string | undefined;
  searchKey: string | undefined;
}): JSX.Element {
  const config = useConfig();
  return (
    <Flex
      vAlign="center"
      hAlign="center"
      column
      gap="gap.smaller"
      className="no-question-wrapper"
    >
      {searchKey && <SearchKeyPresent searchKey={searchKey} />}
      {!searchKey && (
        <NoSearchKey
          selectedTab={selectedTab}
          selectedCourseUser={selectedCourseUser}
        />
      )}
      {selectedTab !== 1 &&
        (selectedCourseUser === '' || selectedCourseUser === undefined) && (
          <Flex vAlign="center" hAlign="center" column gap="gap.small">
            <Text
              size="smallest"
              content={
                <FormattedMessage
                  id="noQuestions.callToAction"
                  description="Call to action for user to post the first question for the course"
                  defaultMessage="Go to team and @{botAppName} to start asking questions"
                  values={{
                    botAppName: config.botAppName,
                  }}
                />
              }
            />
            <Button
              styles={{ width: '17.5rem' }}
              content={
                <FormattedMessage
                  id="noQuestions.askQuestionAction"
                  description="Deeplink to the selected course or channel"
                  defaultMessage="Ask a question"
                />
              }
              onClick={() => {
                microsoftTeams.executeDeepLink(
                  `https://teams.microsoft.com/l/channel/${
                    !channelId ? selectedCourseId : channelId
                  }/groupId=${selectedCourseId}`,
                );
              }}
              primary
            />
          </Flex>
        )}
    </Flex>
  );
}

export default NoQuestions;
