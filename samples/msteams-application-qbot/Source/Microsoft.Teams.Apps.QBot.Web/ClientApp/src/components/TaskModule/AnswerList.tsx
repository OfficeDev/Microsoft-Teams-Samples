import {
  Avatar,
  Flex,
  Button,
  List,
  Text,
  Image,
  ChevronEndIcon,
  FlexItem,
  ListItem,
  Divider,
} from '@fluentui/react-northstar';
import * as microsoftTeams from '@microsoft/teams-js';
import { usePushRelativePath } from 'hooks';
import React, { useMemo, useState, useEffect } from 'react';
import { FormattedMessage } from 'react-intl';
import { useDispatch, useSelector } from 'react-redux';
import { useActionCreator } from 'actionCreators';
import {
  selectCourseIdFromUrl,
  selectPathQuestionId,
  selectPathChannelId,
  selectAnswersForCurrentQuestion,
  selectUsersByAadId,
  selectIfAnswersFirstLoading,
  selectCurrentPathQuestion,
  selectGlobalErrorState,
} from 'selectors';
import { User, Answer } from '../../models';
import { ShowMoreComp } from 'components/CourseQuestions/ShowMoreComp';
import { GlobalErrorComp } from 'components/GlobalErrorComp';
export interface ContentDataProps {
  answer: any;
  users: Map<string, User>;
}

export const ContentData = ({
  answer,
  users,
}: // eslint-disable-next-line sonarjs/cognitive-complexity
ContentDataProps): JSX.Element => {
  const [showMore, setShowMore] = useState<boolean>(false);
  const { message: text } = answer;

  const getText = (num: number) => {
    // For Text that is shorter than desired length
    if (text.length <= num) return text;
    // If text is longer than desired length & showMore is true
    if (text.length > num && showMore) {
      return (
        <>
          {text}
          <Button
            primary
            className="answer-see-less"
            styles={{ minWidth: '2.5rem', padding: '0 0 2px 0', height: '0' }}
            size="medium"
            text
            content={
              <FormattedMessage
                id="answerList.seeLess"
                description="Collapse the long text"
                defaultMessage="See less"
              />
            }
            onClick={(e) => {
              e.stopPropagation();
              setShowMore(false);
            }}
          />
        </>
      );
    }
    // If text is longer than desired length & showMore is false
    if (text.length > num) {
      return (
        <>
          {`${text.slice(0, num)}...`}
          <Button
            primary
            className="answer-see-more"
            styles={{ minWidth: '2.5rem', padding: '0 0 1px 0', height: '0' }}
            size="medium"
            text
            content={
              <FormattedMessage
                id="answerList.seeMore"
                description="Expands the truncated text."
                defaultMessage="See more"
              />
            }
            onClick={(e) => {
              e.stopPropagation();
              setShowMore(true);
            }}
          />
        </>
      );
    }
  };
  const user = users.get(answer.authorId);
  return (
    <Flex styles={{ marginBottom: '0.1rem' }}>
      <Flex column gap="gap.smaller">
        <Flex space="between" vAlign="center" hAlign="center">
          <Text
            styles={{ paddingLeft: '0.3rem' }}
            size="medium"
            content={getText(170)}
          />
        </Flex>
        <Flex styles={{ paddingLeft: '0.3rem' }}>
          <Avatar size="smallest" image={user?.iconUrl} name={user?.name} />
          <Text style={{ padding: '5px 8px' }} content={user?.name} />
        </Flex>
      </Flex>
      <FlexItem push align="center">
        <ChevronEndIcon />
      </FlexItem>
    </Flex>
  );
};

// eslint-disable-next-line max-lines-per-function
export function AnswerList(): JSX.Element {
  const dispatch = useDispatch();
  const push = usePushRelativePath();
  const courseId = useSelector(selectCourseIdFromUrl);
  const questionId = useSelector(selectPathQuestionId);
  const channelId = useSelector(selectPathChannelId);
  const answersData = useSelector(selectAnswersForCurrentQuestion);
  const users = useSelector(selectUsersByAadId);
  const areAnswersLoading = useSelector(selectIfAnswersFirstLoading);
  const question = useSelector(selectCurrentPathQuestion);
  const isErrorOccurred = useSelector(selectGlobalErrorState);
  const loadCourseQuestions = useActionCreator(
    (s) => s.question.loadCourseQuestions,
  );
  useEffect(() => {
    if (courseId && !question) {
      loadCourseQuestions({ courseId });
    }
  }, [loadCourseQuestions, courseId]);

  const answerItems = useMemo(
    () =>
      answersData.map((answer: Answer) => (
        <>
          <ListItem
            styles={{
              paddingLeft: '0',
              paddingRight: '0',
              marginRight: '0.5rem',
            }}
            content={<ContentData answer={{ ...answer }} users={users} />}
            data-id={answer.id}
            index={parseInt(answer.id)}
            onClick={() => {
              if (
                courseId &&
                questionId &&
                answersData &&
                answersData.length > 0
              ) {
                dispatch(
                  push(
                    `/course/${courseId}/channel/${channelId}/question/${questionId}/selectedResponse/${answer.id}`,
                  ),
                );
              }
            }}
          />
          <Divider styles={{ paddingRight: '0.5rem' }} />
        </>
      )),
    [users, answersData],
  );

  if (areAnswersLoading) return <></>;

  return isErrorOccurred || answersData.length > 0 ? (
    <Flex
      gap="gap.smaller"
      column
      styles={{
        paddingLeft: '1.8rem',
        maxHeight: '100%',
        paddingTop: '0.5rem',
      }}
    >
      {isErrorOccurred && <GlobalErrorComp />}
      {answersData.length > 0 && question && (
        <>
          <Text
            size="medium"
            weight="semibold"
            content={
              <FormattedMessage
                id="answerList.questionHeader"
                description="Display question header"
                defaultMessage="Question:"
              />
            }
          />
          <ShowMoreComp text={question.messageText} num={170} />
        </>
      )}
      {answersData.length > 0 && (
        <List
          styles={{ padding: 0, height: '100%', overflowY: 'scroll' }}
          selectable
        >
          {answerItems}
        </List>
      )}
    </Flex>
  ) : (
    <Flex
      column
      styles={{
        paddingLeft: '1.8rem',
        paddingRight: '1.5rem',
        minHeight: 'calc(100vh - 1rem)',
      }}
    >
      <Flex
        styles={{ paddingRight: '1rem', flexGrow: 1 }}
        vAlign="center"
        hAlign="center"
        column
        gap="gap.large"
      >
        <Image src={'/NoAnswers.png'} />
        <Text
          styles={{ marginBottom: '0.2rem !important' }}
          size="large"
          weight="bold"
          content={
            <FormattedMessage
              id="answerList.errorMessage"
              description="Display error message when there are no answers to select"
              defaultMessage="There are no responses to select from."
            />
          }
        />
        <Text
          size="smallest"
          content={
            <FormattedMessage
              id="answerList.errorMessageSub"
              description="Display error message when there are no answers to select"
              defaultMessage="Responses will show up here"
            />
          }
        />
      </Flex>
      <FlexItem push align="end">
        <Button
          primary
          content={
            <FormattedMessage
              id="answerList.okay"
              description="To close the task module"
              defaultMessage="Okay"
            />
          }
          onClick={() => microsoftTeams.tasks.submitTask(undefined)}
          type="submit"
          size="medium"
        />
      </FlexItem>
    </Flex>
  );
}
