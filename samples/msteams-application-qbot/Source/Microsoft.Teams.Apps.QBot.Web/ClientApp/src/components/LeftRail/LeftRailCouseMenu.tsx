import React, { useCallback, useEffect } from 'react';
import { LeftRailMenu } from './LeftRailMenu';
import {
  Image,
  Button,
  MenuItem,
  Flex,
  FlexItem,
  Box,
  useCSS,
  Text,
} from '@fluentui/react-northstar';
import { Course } from 'models';
import { CourseSettingsIcon } from './CourseSettingsIcon';
import { useDispatch, useSelector } from 'react-redux';
import { usePushRelativePath } from 'hooks';
import { useActionCreator } from 'actionCreators';
import {
  selectCourses,
  selectCurrentPathCourseIndex,
  selectIfFirstLoadingCourses,
} from 'selectors';
import { useVirtualizedItemRenderer } from 'hooks/useVirtualizedItemRenderer';
import { AutoSizer } from 'react-virtualized';
import { useTeamsContext } from 'components/TeamsProvider/hooks';

const leftRailCourseMenuItemStyles = {
  ':hover': {
    '& .more-icon': {
      visibility: 'visible',
    },
  },
  '& .more-icon': {
    visibility: 'hidden',
  },
  paddingLeft: 0,
  paddingRight: 0,
  '& img': {
    borderRadius: '.3rem',
    height: '1.2rem',
    width: '1.2rem',
    marginRight: '1em',
    padding: 0,
  },
  '& button': {
    border: 0,
  },
};

// eslint-disable-next-line max-lines-per-function
function LeftRailCourseMenuItem({ course }: { course: Course }) {
  const containerClassName = useCSS(leftRailCourseMenuItemStyles);
  return (
    <Flex
      gap="gap.small"
      className={containerClassName}
      vAlign="center"
      hAlign="start"
    >
      <Button
        text
        content={
          <>
            <Image src={course.iconUrl} />
            <Text>{course.displayName}</Text>
          </>
        }
        size="small"
      />
      <FlexItem push>
        <Box className="more-icon">
          <CourseSettingsIcon course={course} />
        </Box>
      </FlexItem>
    </Flex>
  );
}

// eslint-disable-next-line sonarjs/cognitive-complexity
export function LeftRailCourseMenu(): JSX.Element {
  const dispatch = useDispatch();
  const courses = useSelector(selectCourses);
  const isFirstLoad = useSelector(selectIfFirstLoadingCourses);
  const activeIndex = useSelector(selectCurrentPathCourseIndex);
  const push = usePushRelativePath();
  const loadCoursesCommand = useActionCreator((s) => s.course.loadCourses);
  const teamsContext = useTeamsContext();

  useEffect(() => {
    // When this component loads, make sure to load all the courses
    loadCoursesCommand();
  }, [loadCoursesCommand]);
  useEffect(() => {
    // don't navigate if we have a subentityid since that should take precedence
    if (teamsContext.subEntityId) return;
    if (activeIndex < 0 && courses.length > 0) {
      console.log('No active, course, navigating to first course');
      // initial load scenario (load thee default course)
      dispatch(push(`/courses/${courses[0].teamAadObjectId}`));
    }
  }, [dispatch, activeIndex, courses, push, teamsContext.subEntityId]);
  const onNewItemSelected = useCallback(
    (course: Course) => {
      dispatch(push(`/courses/${course.teamAadObjectId}`));
    },
    [dispatch, push],
  );
  // TODO(nibeauli): have this rener a skeleton when course isn't defined
  const renderRow = useVirtualizedItemRenderer<Course, typeof MenuItem>(
    courses,
    (Component, props, course) => (
      <Component
        {...props}
        key={course ? course.id : props.index}
        styles={{
          ...props.style,
          paddingBottom: 0,
          paddingTop: 0,
          border: 0,
        }}
        onClick={() => course && onNewItemSelected(course)}
      >
        {course ? <LeftRailCourseMenuItem course={course} /> : null}
      </Component>
    ),
    [onNewItemSelected],
  );
  return (
    <AutoSizer>
      {({ width, height }) => (
        <LeftRailMenu
          activeIndex={activeIndex}
          rowCount={isFirstLoad ? 5 : courses.length}
          rowRenderer={renderRow}
          height={height}
          width={width}
        />
      )}
    </AutoSizer>
  );
}

export default LeftRailCourseMenu;
