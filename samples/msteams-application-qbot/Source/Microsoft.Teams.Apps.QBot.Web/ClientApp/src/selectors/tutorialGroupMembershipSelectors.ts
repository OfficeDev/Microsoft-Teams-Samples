import { QBotState } from 'compositionRoot';
import { groupBy } from 'lodash';
import { createSelector } from 'reselect';
import { tutorialGroupMemberAdapter } from 'stores/tutorialGroupMembers';

const tutorialGroupMemberSelectors = tutorialGroupMemberAdapter.getSelectors(
  (state: QBotState) => state.tutorialGroupMembers,
);
// Don't default export for selectors
// eslint-disable-next-line import/prefer-default-export
export const selectTutorialGroupMemberships = createSelector(
  tutorialGroupMemberSelectors.selectAll,
  (allMemberObjects) => allMemberObjects.flatMap((m) => m.memberships),
);

export const selectTutorialGroupMembershipsByCourseId = createSelector(
  selectTutorialGroupMemberships,
  (memberships) => {
    const membershipsByCourseId = groupBy(memberships, (m) => m.courseId);
    return new Map(Object.entries(membershipsByCourseId));
  },
);

export const selectIfTutorialGroupMembershipsFirstLoading = createSelector(
  selectTutorialGroupMemberships,
  (state: QBotState) => state.tutorialGroupMembers.loading,
  (tutorialGroupMemberships, isLoading) =>
    tutorialGroupMemberships.length === 0 && isLoading,
);
