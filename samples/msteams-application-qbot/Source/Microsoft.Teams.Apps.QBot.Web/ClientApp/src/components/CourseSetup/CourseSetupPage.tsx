import React, { useEffect, useState } from 'react';
import { Flex, Menu, useCSS, Box } from '@fluentui/react-northstar';
import { useDefaultColorScheme } from 'hooks/useColorScheme';
import { Route, Switch, useRouteMatch } from 'react-router';
import { useDispatch, useSelector } from 'react-redux';
import { usePushRelativePath } from 'hooks';
import { trackPageComponent } from 'appInsights';
import TutorialGroupConfigurationPage from 'components/TutorialGroupManagement';
import MembershipConfigurationPage from 'components/MembershipConfiguration';
import { FormattedMessage, defineMessages, useIntl } from 'react-intl';
import { CourseGeneralConfigurationPage } from './CourseGeneralConfigurationPage';
import { useTeamsContext } from 'components/TeamsProvider/hooks';
import {
  selectCurrentCourseUserMemberships,
  selectCurrentPathCourse,
} from 'selectors';
import { CourseMemberRole } from 'models';
import { useActionCreator } from 'actionCreators';

const courseSetupPageMessages = defineMessages({
  menuAriaLabel: {
    id: 'courseSetupPage.menuAriaLabel',
    defaultMessage: 'Course configuration page',
    description:
      'WAIARIA label for the menu for picking between the course configuration pages',
  },
});

// eslint-disable-next-line max-lines-per-function
function CourseSetupPage(): JSX.Element {
  const push = usePushRelativePath();
  const dispatch = useDispatch();
  const intl = useIntl();
  const colorScheme = useDefaultColorScheme();
  const [toolbarRef, setToolbarRef] = useState<HTMLDivElement | null>(null);
  const match = useRouteMatch<{ courseId: string; configPage: string }>(
    '/personal/courses/:courseId/configure/:configPage',
  );

  const loadCourseMembers = useActionCreator(
    (s) => s.courseMember.loadCourseMembers,
  );

  const userMemberships = useSelector(selectCurrentCourseUserMemberships);
  const course = useSelector(selectCurrentPathCourse);
  useEffect(() => {
    if (!course?.id) return;
    loadCourseMembers({ courseId: course.id });
  }, [course?.id, loadCourseMembers]);

  const teamsContext = useTeamsContext();
  const currentUserId = teamsContext.userObjectId;
  const [courseRole, setCourseRole] = useState<CourseMemberRole>('Student');

  useEffect(() => {
    if (course) {
      const currentUserMembership = userMemberships.find(
        (m) => m.user.aadId === currentUserId && m.course.id === course.id,
      );
      setCourseRole(currentUserMembership?.role || 'Student');
    }
  }, [course, currentUserId, setCourseRole, userMemberships]);

  const onMenuChange = (pageSlug: string) => {
    return () => {
      dispatch(
        push(`/courses/${match?.params.courseId}/configure/${pageSlug}`),
      );
    };
  };
  const pageClass = useCSS({
    width: '100%',
    height: '100%',
    backgroundColor: colorScheme.background2,
    marginLeft: '1rem',
    marginTop: '1rem',
    paddingTop: '0.625rem',
    paddingLeft: '1rem',
    paddingRight: '1rem',
  });

  const allMenuItems = [
    {
      key: 'general',
      content: (
        <FormattedMessage
          id="courseSetupPage.generalMenuItem"
          description="Menu item for picking general settings page"
          defaultMessage="General"
        />
      ),
      onClick: onMenuChange('general'),
      allowedRoles: ['Educator'],
    },
    {
      key: 'members',
      content: (
        <FormattedMessage
          id="courseSetupPage.membersMenuItem"
          description="Menu item for picking tutorial groups settings page"
          defaultMessage="Members"
        />
      ),
      onClick: onMenuChange('members'),
    },
    {
      key: 'tutorialGroups',
      content: (
        <FormattedMessage
          id="courseSetupPage.tutorialGroupsMenuItem"
          description="Menu item for picking tutorial groups settings page"
          defaultMessage="Tutorial Groups"
        />
      ),
      onClick: onMenuChange('tutorialGroups'),
      allowedRoles: ['Educator'],
    },
  ];
  const menuItems = allMenuItems.filter(
    (mi) => !mi.allowedRoles || mi.allowedRoles.indexOf(courseRole) >= 0,
  );

  const index = menuItems.findIndex(
    (item) => item.key.toUpperCase() === match?.params.configPage.toUpperCase(),
  );
  if (index < 0) {
    // Go to the first valid configuration pane if we are on an invalid section
    onMenuChange(menuItems[0].key)();
  }

  return (
    <Flex column className={pageClass} gap="gap.medium">
      <Flex>
        <Menu
          activeIndex={index}
          items={menuItems}
          underlined
          primary
          aria-label={intl.formatMessage(
            courseSetupPageMessages.menuAriaLabel,
            {},
          )}
        />
        <Flex.Item push>
          <Box styles={{ paddingRight: '1rem' }} ref={setToolbarRef} />
        </Flex.Item>
      </Flex>

      <Switch>
        <Route path="/personal/courses/:courseId/configure/general">
          {toolbarRef && <CourseGeneralConfigurationPage />}
        </Route>
        <Route path="/personal/courses/:courseId/configure/members">
          {toolbarRef && (
            <MembershipConfigurationPage
              currentCourseRole={courseRole}
              toolbarRef={toolbarRef}
            />
          )}
        </Route>
        <Route path="/personal/courses/:courseId/configure/tutorialGroups">
          {toolbarRef && (
            <TutorialGroupConfigurationPage toolbarRef={toolbarRef} />
          )}
        </Route>
      </Switch>
    </Flex>
  );
}
export default trackPageComponent(CourseSetupPage);
