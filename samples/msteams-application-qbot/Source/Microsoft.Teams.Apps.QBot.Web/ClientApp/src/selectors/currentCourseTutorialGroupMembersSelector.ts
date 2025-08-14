import { createSelector } from 'reselect';
import { selectCurrentPathCourse } from './currentCourseSelectors';
import { selectTutorialGroupMembershipsByCourseId } from './tutorialGroupMembershipSelectors';
import { selectTutorialGroups } from './tutorialGroupSelectors';
import { selectUsersById } from './userSelectors';
import { groupBy } from 'lodash';
import { isNotNullOrUndefined } from 'util/isNotNullOrUndefined';

export const currentCourseTutorialGroupMembersSelector = createSelector(
  selectCurrentPathCourse,
  selectTutorialGroups,
  selectTutorialGroupMembershipsByCourseId,
  selectUsersById,
  (course, tutorialGroups, tutorialGroupMemberships, usersById) => {
    if (!course) {
      console.warn('No course loaded');
      return [];
    }
    const courseMemberships = course
      ? tutorialGroupMemberships.get(course.id)
      : [];
    const courseTutorialGroups = tutorialGroups.filter(
      (tutorialGroup) => tutorialGroup.courseId === course?.id,
    );
    const membershipsByTutorialGroupId = groupBy(
      courseMemberships,
      (membership) => membership.tutorialGroupId,
    );
    return courseTutorialGroups.map((tutorialGroup) => {
      const memberships = membershipsByTutorialGroupId[tutorialGroup.id] ?? [];
      const members = memberships
        .map((member) => usersById.get(member.userId))
        .filter(isNotNullOrUndefined);
      return {
        tutorialGroup,
        members,
      };
    });
  },
);
