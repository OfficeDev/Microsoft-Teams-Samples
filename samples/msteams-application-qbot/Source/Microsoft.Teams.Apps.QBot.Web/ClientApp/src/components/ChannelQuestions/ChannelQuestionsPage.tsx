import {
  Flex,
  Input,
  Menu,
  SearchIcon,
  useCSS,
} from '@fluentui/react-northstar';
import {
  useEqualityFilter,
  useFilterableList,
  useFilter,
  useSubstringFilter,
  useStatefulFilter,
} from 'hooks/useFilterableList';
import { FullQuestion, User } from 'models';
import React, { useCallback, useEffect } from 'react';
import { useSelector } from 'react-redux';
import {
  selectCurrentPathCourse,
  selectUsersByCourseId,
  selectChannelIdFromPath,
  selectIsQuestionsLoading,
  selectGlobalErrorState,
  selectCurrentCourseQuestions,
} from 'selectors';
import CourseQuestion from '../CourseQuestions/CourseQuestion';
import { trackPageComponent } from 'appInsights';
import NoQuestions from 'components/CourseQuestions/NoQuestions';
import SkeletonCourseQuestions from 'components/CourseQuestions/SkeletonCourseQuestions';
import { UserPicker } from 'components/CourseQuestions/UserPicker';
import { useDefaultColorScheme } from 'hooks/useColorScheme';
import { FormattedMessage, defineMessages, useIntl } from 'react-intl';
import { useActionCreator } from 'actionCreators';
import { GlobalErrorComp } from 'components/GlobalErrorComp';

const channelQuestionsPageMessages = defineMessages({
  searchQuestionPlaceholder: {
    id: 'channelQuestionsPage.searchQuestionPlaceholder',
    defaultMessage: 'Search questions...',
    description: 'Placeholder for search question filter.',
  },
  courseMenuAriaLabel: {
    id: 'channelQuestionsPage.courseMenuAriaLabel',
    defaultMessage: 'courses questions',
    description: 'Course questions aria-label.',
  },
});

// eslint-disable-next-line max-lines-per-function, sonarjs/cognitive-complexity
function ChannelQuestionsPage(): JSX.Element {
  const loadCourseQuestions = useActionCreator(
    (s) => s.question.loadCourseQuestions,
  );
  const loadCourseMembers = useActionCreator(
    (s) => s.courseMember.loadCourseMembers,
  );
  const intl = useIntl();
  const course = useSelector(selectCurrentPathCourse);
  const isQuestionsLoading = useSelector(selectIsQuestionsLoading);
  const channelId = useSelector(selectChannelIdFromPath);
  const courseQuestions = useSelector(selectCurrentCourseQuestions);
  const colorScheme = useDefaultColorScheme();
  const usersByCourse = useSelector(selectUsersByCourseId);
  const isErrorOccurred = useSelector(selectGlobalErrorState);
  const faded = useCSS({
    opacity: 0.75,
    filter: 'grayscale(60%)',
  });
  const selectedChannelQuestionFilter = useFilter(
    (q: FullQuestion) => q.channelId === channelId,
    [channelId],
  );

  const {
    filter: answeredQuestionFilter,
    setState: setIsAnsweredSelected,
    state: isAnsweredSelected,
  } = useStatefulFilter(
    (q: FullQuestion, isAnsweredSelected: boolean) =>
      (isAnsweredSelected && q.answer !== undefined) ||
      (!isAnsweredSelected && q.answer === undefined),
    false as boolean, // strange cast since typescript will type this as 'false' not 'boolean'
  );

  const {
    filter: searchQuestionFilter,
    query: searchKey,
    setQuery: setSearchKey,
  } = useSubstringFilter((q: FullQuestion) => [q.messageText], undefined, {
    caseInsensitive: true,
  });
  const {
    filter: selectedUserFilter,
    setState: setSelectedUserId,
    state: selectedUserId,
  } = useEqualityFilter((q: FullQuestion) => q.authorId);

  const selectedCourseQuestionsFilter = useFilter(
    (question: FullQuestion) => question.courseId === course?.id,
    [course?.id],
  );

  const items = [
    {
      key: 'unanswered',
      content: (
        <FormattedMessage
          id="courseQuestionsPage.unansweredTab"
          description="Unanswered tab to diaplay unaswered questions"
          defaultMessage="Unanswered questions"
        />
      ),
      onClick: useCallback(() => setIsAnsweredSelected(false), [
        setIsAnsweredSelected,
      ]),
    },
    {
      key: 'answered',
      content: (
        <FormattedMessage
          id="courseQuestionsPage.answeredTab"
          description="Answered tab to diaplay aswered questions"
          defaultMessage="Answered questions"
        />
      ),
      onClick: useCallback(() => setIsAnsweredSelected(true), [
        setIsAnsweredSelected,
      ]),
    },
  ];

  const courseUsers = course ? usersByCourse[course.id] ?? [] : [];
  const handleUserChange = useCallback(
    (user?: User) => {
      setSelectedUserId(user?.id);
    },
    [setSelectedUserId],
  );

  const handleSearchChange = useCallback(
    (evt: unknown, data?: { value: string }) => {
      setSearchKey(data?.value);
    },
    [setSearchKey],
  );
  useEffect(() => {
    if (!!course?.id) {
      loadCourseQuestions({ courseId: course.id });
      loadCourseMembers({ courseId: course.id });
    }
  }, [loadCourseQuestions, loadCourseMembers, course?.id]);

  const currentTabQuestions = useFilterableList(
    courseQuestions,
    answeredQuestionFilter,
    selectedChannelQuestionFilter,
  );
  const filteredQuestions = useFilterableList(
    currentTabQuestions,
    selectedUserFilter,
    searchQuestionFilter,
    selectedCourseQuestionsFilter,
  );
  const currentCourseCurrentTabQuestions = useFilterableList(
    courseQuestions,
    answeredQuestionFilter,
  );
  const currentCourseQuestions = useFilterableList(
    currentCourseCurrentTabQuestions,
  );

  const selectedTab = isAnsweredSelected ? 1 : 0;
  if (!course) {
    return <> </>;
  }
  let child: JSX.Element;
  const isFilterDisabled =
    filteredQuestions.length === 0 && searchKey === undefined ? true : false;
  if (isQuestionsLoading) {
    child = <SkeletonCourseQuestions selectedTab={selectedTab} />;
  } else if (filteredQuestions.length === 0) {
    child = (
      <NoQuestions
        searchKey={searchKey}
        selectedCourseId={course?.id}
        selectedTab={selectedTab}
        selectedCourseUser={selectedUserId}
        channelId={channelId}
      />
    );
  } else {
    child = (
      <CourseQuestion
        questions={filteredQuestions}
        selectedCourseName={course?.displayName ?? ''}
      />
    );
  }
  return (
    <>
      {isErrorOccurred && <GlobalErrorComp />}
      <Flex className="personal-dashboard">
        <Flex
          column
          gap="gap.medium"
          padding="padding.medium"
          className="course-menu"
          style={{
            backgroundColor: colorScheme.background2,
          }}
        >
          <Flex space="between" className="tabs">
            <Menu
              activeIndex={selectedTab}
              items={items}
              underlined
              primary
              aria-label={intl.formatMessage(
                channelQuestionsPageMessages.courseMenuAriaLabel,
                {},
              )}
            />
            <Input
              icon={<SearchIcon />}
              inverted
              placeholder={intl.formatMessage(
                channelQuestionsPageMessages.searchQuestionPlaceholder,
                {},
              )}
              onChange={handleSearchChange}
              disabled={isFilterDisabled}
            />
          </Flex>
          <Flex className="filters" gap="gap.medium">
            <UserPicker
              users={courseUsers}
              questions={currentCourseQuestions}
              disabledClassName={faded}
              onUserSelect={handleUserChange}
              disabled={isFilterDisabled}
            />
          </Flex>
          {child}
        </Flex>
      </Flex>
    </>
  );
}

export default trackPageComponent(ChannelQuestionsPage);
