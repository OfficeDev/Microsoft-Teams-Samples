import { User } from 'models';
import { createSelector } from 'reselect';
import { QBotState } from 'compositionRoot';
import { groupBy, mapValues } from 'lodash';

import { selectUsersById } from './userSelectors';
import { courseMemberAdapter } from 'stores/courseMembers';

const courseMemberSelectors = courseMemberAdapter.getSelectors(
  (state: QBotState) => state.courseMembers,
);
// Don't default export for selectors
// eslint-disable-next-line import/prefer-default-export
export const selectCourseMemberships = courseMemberSelectors.selectAll;

export const selectIfCourseMembershipsFirstLoading = createSelector(
  selectCourseMemberships,
  (state: QBotState) => state.courseMembers.loading,
  (memberships, isLoading) => memberships.length === 0 && isLoading,
);
export const selectCourseMembershipsByCourseId = createSelector(
  selectCourseMemberships,
  (memberships) => {
    const membershipsByCourseId = groupBy(memberships, (m) => m.courseId);
    return new Map(Object.entries(membershipsByCourseId));
  },
);

export const selectUsersByCourseId = createSelector(
  selectCourseMemberships,
  selectUsersById,
  (memberships, usersById) => {
    const membershipsByCourseId = groupBy(memberships, (m) => m.courseId);
    // For each course & list of members
    // convert the members into user objects
    return mapValues(
      membershipsByCourseId,
      (memberships) =>
        memberships
          .map((membership) =>
            usersById.has(membership.userId)
              ? usersById.get(membership.userId)
              : null,
          )
          .filter((x) => x) as User[],
    );
  },
);
