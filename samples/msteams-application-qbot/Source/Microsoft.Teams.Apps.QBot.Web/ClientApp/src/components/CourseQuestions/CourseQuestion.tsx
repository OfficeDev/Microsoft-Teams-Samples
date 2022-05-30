// eslint-disable sonarjs/no-duplicated-branches
// eslint-disable sonarjs/cognitive-complexity,
import {
  ArrowRightIcon,
  Avatar,
  Breadcrumb,
  Button,
  ChevronEndMediumIcon,
  Divider,
  Flex,
  Text,
} from '@fluentui/react-northstar';
import * as microsoftTeams from '@microsoft/teams-js';
import React, { useMemo } from 'react';
import { FullQuestion } from 'models';
import { ShowMoreComp } from './ShowMoreComp';
import { UserIcon } from 'components/UserIcon/UserIcon';
import { useColorScheme, useDefaultColorScheme } from 'hooks/useColorScheme';
import { FormattedMessage, defineMessages, useIntl } from 'react-intl';
import dayjs from 'dayjs';
import relativeTime from 'dayjs/plugin/relativeTime';
import localizedFormat from 'dayjs/plugin/localizedFormat';
dayjs.extend(relativeTime);
dayjs.extend(localizedFormat);

const courseQuestionMessages = defineMessages({
  breadCrumbAriaLabel: {
    id: 'courseQuestion.breadCrumbAriaLabel',
    defaultMessage: 'breadcrumb',
    description: 'Aria label for the breadcrumb.',
  },
});

function QuestionComp({
  selectedQuestion,
  selectedCourseName,
}: {
  selectedQuestion: FullQuestion;
  selectedCourseName: string;
}): JSX.Element {
  const { user, channelId, courseId, messageId, answer } = selectedQuestion;
  const defaultColorScheme = useDefaultColorScheme();
  const intl = useIntl();
  const brandColorScheme = useColorScheme<'foreground1'>('brand');
  const timestampString = useMemo(
    () => dayjs(selectedQuestion.timeStamp).format('L LT'),
    [selectedQuestion.timeStamp],
  );
  const agoString = useMemo(() => dayjs(selectedQuestion.timeStamp).fromNow(), [
    selectedQuestion.timeStamp,
  ]);
  return (
    <Flex
      gap="gap.smaller"
      fill
      className="question"
      column
      key={selectedQuestion.id}
    >
      <Flex gap="gap.smaller" className="user-details">
        <UserIcon
          styles={{ marginRight: '0.3rem !important' }}
          size="smallest"
          user={user}
        />
        <Text size="small" content={user.name} weight="semibold" />
        <Text
          timestamp
          size="smaller"
          title={timestampString}
          content={agoString}
          styles={{
            color: defaultColorScheme.foreground2,
            paddingTop: '0.08rem',
          }}
          weight="light"
        />
      </Flex>
      <Flex
        column
        gap="gap.smaller"
        styles={{ marginBottom: '0.3rem !important' }}
      >
        <Flex space="between" styles={{ marginBottom: '0.23rem !important' }}>
          <Text
            size="large"
            weight="semibold"
            content={selectedQuestion.messageText.trim()}
          />
          <Flex.Item push>
            <Flex vAlign="center" hAlign="center">
              <Button
                className="conversation-button"
                styles={{ color: brandColorScheme.foreground1 }}
                size="small"
                iconPosition="after"
                icon={
                  <ArrowRightIcon
                    size="small"
                    styles={{ marginTop: '0.2rem' }}
                  />
                }
                text
                content={
                  <FormattedMessage
                    id="courseQuestion.conversation"
                    description="Deeplink to the conversation"
                    defaultMessage="Go to conversation"
                  />
                }
                onClick={() => {
                  microsoftTeams.executeDeepLink(
                    `https://teams.microsoft.com/l/message/${channelId}/${messageId}?parentMessageId=${messageId}&groupId=${courseId}`,
                  );
                }}
              />
            </Flex>
          </Flex.Item>
        </Flex>
        <Breadcrumb
          aria-label={intl.formatMessage(
            courseQuestionMessages.breadCrumbAriaLabel,
            {},
          )}
          size="small"
        >
          <Avatar
            className="course-icon"
            size="smallest"
            name={selectedCourseName}
            square
          />
          <Breadcrumb.Item
            aria-current="page"
            styles={{
              color: defaultColorScheme.foreground2,
              paddingLeft: '0.1rem',
              paddingRight: '0.3rem',
            }}
          >
            {selectedCourseName}
          </Breadcrumb.Item>
          <Breadcrumb.Divider>
            <ChevronEndMediumIcon
              styles={{ color: defaultColorScheme.foreground2 }}
              size="smaller"
            />
          </Breadcrumb.Divider>
          <Breadcrumb.Item
            styles={{
              color: defaultColorScheme.foreground2,
              paddingLeft: '0.3rem',
            }}
            aria-current="page"
          >
            {selectedQuestion.channelName}
          </Breadcrumb.Item>
        </Breadcrumb>
        {answer && <ShowMoreComp text={answer?.message} />}
      </Flex>
      <Divider />
    </Flex>
  );
}

function renderQuestion({
  question,
  selectedCourseName,
}: {
  question: FullQuestion;
  selectedCourseName: string;
}) {
  return (
    <Flex styles={{ marginBottom: '0.6rem !important' }} key={question.id} fill>
      <QuestionComp
        key={question.id}
        selectedQuestion={question}
        selectedCourseName={selectedCourseName}
      />
    </Flex>
  );
}

export default function CourseQuestion({
  questions,
  selectedCourseName,
}: {
  questions: FullQuestion[];
  selectedCourseName: string;
}): JSX.Element {
  return (
    <Flex column className="questions-wrapper" gap="gap.medium">
      {questions.map((question) =>
        renderQuestion({
          question,
          selectedCourseName,
        }),
      )}
    </Flex>
  );
}
