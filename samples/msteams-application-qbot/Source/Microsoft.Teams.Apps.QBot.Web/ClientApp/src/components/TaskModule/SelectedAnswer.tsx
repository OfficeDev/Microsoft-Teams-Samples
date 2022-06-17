import {
  Button,
  Checkbox,
  Flex,
  Header,
  Label,
  TextArea,
  Avatar,
  Text,
} from '@fluentui/react-northstar';
import * as microsoftTeams from '@microsoft/teams-js';
import React, { useState, useCallback, useEffect } from 'react';
import { useSelector } from 'react-redux';
import {
  selectAnswerForCurrentQuestion,
  selectCourseIdFromUrl,
  selectPathQuestionId,
  selectPathChannelId,
  selectUsersByAadId,
  selectIsAnswerPosting,
  selectPathAnswerId,
  selectGlobalErrorState,
} from 'selectors';
import { useTeamsContext } from 'components/TeamsProvider/hooks';
import { FormattedMessage } from 'react-intl';
import { useActionCreator } from 'actionCreators';
import { GlobalErrorComp } from 'components/GlobalErrorComp';

export function SelectedAnswer(): JSX.Element {
  const postAnswer = useActionCreator((s) => s.answer.postAnswer);
  const answerId = useSelector(selectPathAnswerId);
  const loadAnswer = useActionCreator((s) => s.answer.loadAnswer);
  const users = useSelector(selectUsersByAadId);
  const answer = useSelector(selectAnswerForCurrentQuestion);
  const courseId = useSelector(selectCourseIdFromUrl);
  const questionId = useSelector(selectPathQuestionId);
  const channelId = useSelector(selectPathChannelId);
  const isAnswerPosting = useSelector(selectIsAnswerPosting);
  const [messageText, updateMessage] = useState(answer?.message || '');
  const [isChecked, setIsChecked] = useState(true);
  const context = useTeamsContext();
  const userId = context.userObjectId;
  const user = users.get(answer?.authorId || '');
  const isErrorOccurred = useSelector(selectGlobalErrorState);

  useEffect(() => {
    if (!answer && courseId && channelId && questionId && answerId) {
      loadAnswer({ courseId, channelId, questionId, answerId });
    }
    if (answer) {
      updateMessage(answer?.message);
    }
  }, [loadAnswer, answer, courseId, channelId, questionId, answerId]);

  return (
    <Flex
      column
      gap="gap.small"
      padding="padding.medium"
      styles={{
        paddingLeft: '1.8rem',
        paddingRight: '1.5rem',
        minHeight: 'calc(100vh - 1rem)',
      }}
    >
      {isErrorOccurred && <GlobalErrorComp />}
      <Header as="h3">
        <FormattedMessage
          id="selectedAnswer.headerText"
          description="Display selected answer."
          defaultMessage="Confirm answer"
        />
      </Header>
      <Label
        color="white"
        content={
          <FormattedMessage
            id="selectedAnswer.answer"
            description="Display selected answer."
            defaultMessage="Confirm answer"
          />
        }
      />
      <TextArea
        resize="vertical"
        disabled={isAnswerPosting}
        value={messageText.trim()}
        onChange={(e) => {
          const target = e.target as HTMLTextAreaElement;
          updateMessage(target?.value);
        }}
        variables={{ height: '120px' }}
      />
      <Flex>
        <Checkbox
          disabled={isAnswerPosting || !messageText}
          checked={isChecked}
          onChange={useCallback(() => setIsChecked(!isChecked), [
            isChecked,
            setIsChecked,
          ])}
          label={
            <FormattedMessage
              id="selectedAnswer.checkboxText"
              description="Check box to display user name of the selected answer"
              defaultMessage="Show answered by"
            />
          }
        />
        <Avatar
          styles={{ marginTop: '0.3rem' }}
          size="smallest"
          image={user?.iconUrl}
          name={user?.name}
        />
        <Text style={{ padding: '4px 8px' }} content={user?.name} />
      </Flex>
      <Flex hAlign="end" vAlign="end" gap="gap.small" styles={{ flexGrow: 2 }}>
        <Button
          secondary
          content={
            <FormattedMessage
              id="selectedAnswer.cancelAction"
              description="To close the taskmodule "
              defaultMessage="Cancel"
            />
          }
          onClick={() => microsoftTeams.tasks.submitTask(undefined)}
          type="submit"
          size="medium"
        />
        <Button
          primary
          loading={isAnswerPosting}
          disabled={isAnswerPosting || !messageText}
          content={
            <FormattedMessage
              id="selectedAnswer.postAnswer"
              description="Post the selected answer"
              defaultMessage="Post answer"
            />
          }
          onClick={() => {
            if (!courseId || !channelId || !questionId || !answer) {
              console.warn('Null parameter, ignoring click', {
                courseId,
                channelId,
                questionId,
                answer,
              });
              return;
            }
            if (answer) {
              answer.message = messageText;
              answer.acceptedById = userId;
              answer.authorId = (!isChecked && userId) || answer.authorId;
            }
            postAnswer({
              courseId,
              channelId,
              questionId,
              answer,
            });
          }}
          type="submit"
          size="medium"
        />
      </Flex>
    </Flex>
  );
}
