import { groupBy } from 'lodash';
import { UserCourseMemberships } from 'models';
import { createSelector } from 'reselect';
import { isNotNullOrUndefined } from 'util/isNotNullOrUndefined';
import { selectCourseMembershipsByCourseId } from './courseMembersSelectors';
import { selectCourses } from './courseSelectors';
import { selectTutorialGroupMembershipsByCourseId } from './tutorialGroupMembershipSelectors';
import { selectTutorialGroupsById } from './tutorialGroupSelectors';
import { selectUsersById } from './userSelectors';

// eslint-disable-next-line import/prefer-default-export
export const selectUserCourseMemberships = createSelector(
  selectCourses,
  selectUsersById,
  selectCourseMembershipsByCourseId,
  selectTutorialGroupMembershipsByCourseId,
  selectTutorialGroupsById,
  (
    courses,
    usersById,
    courseMembershipsById,
    tutorialGroupMembershipsByCourseId,
    tutorialGroupsById,
  ): UserCourseMemberships[] => {
    return courses.flatMap((course) => {
      const memberships = courseMembershipsById.get(course.id) ?? [];
      const tutorialGroupMemberships =
        tutorialGroupMembershipsByCourseId.get(course.id) ?? [];

      return memberships
        .flatMap((membership) => {
          const user = usersById.get(membership.userId);
          if (!user) return undefined;
          const tutorialGroups = tutorialGroupMemberships
            .filter((tgMembership) => tgMembership.userId === membership.userId)
            .map((tgMembership) =>
              tutorialGroupsById.get(tgMembership.tutorialGroupId),
            )
            .filter(isNotNullOrUndefined);
          return {
            user,
            role: membership.role,
            course,
            tutorialGroups,
          };
        })
        .filter(isNotNullOrUndefined);
    });
  },
);

export const selectUserCourseMembershipsByCourseId = createSelector(
  selectUserCourseMemberships,
  (userCourseMemberships) =>
    new Map(Object.entries(groupBy(userCourseMemberships, (m) => m.course.id))),
);
