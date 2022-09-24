import { TutorialGroupMember } from 'models';
import {
  createSlice,
  createEntityAdapter,
  PayloadAction,
} from '@reduxjs/toolkit';

import { groupBy } from 'lodash';

export interface TutorialGroupMembership {
  userId: string;
  courseId: string;
  memberships: TutorialGroupMember[];
}
export const tutorialGroupMemberAdapter = createEntityAdapter<TutorialGroupMembership>(
  {
    selectId: (membership) => `${membership.courseId}-${membership.userId}`,
    // No logical natural ordering, just order by the user id.
    sortComparer: (a, b) => a.userId.localeCompare(b.userId),
  },
);

const initialState = tutorialGroupMemberAdapter.getInitialState({
  loading: false,
});
export type TutorialGroupMemberState = typeof initialState;

export const tutorialGroupMemberSlice = createSlice({
  name: 'tutorialGroupMembers',
  initialState,
  reducers: {
    tutorialGroupMembersLoading(state) {
      state.loading = true;
    },
    tutorialGroupMembersLoaded(
      state,
      event: PayloadAction<{ members: TutorialGroupMember[] }>,
    ) {
      state.loading = false;
      const membershipsByKey = groupBy(
        event.payload.members,
        (member) => `${member.courseId}-${member.userId}`,
      );
      for (const memberships of Object.values(membershipsByKey)) {
        // we will always have at least one entry, grab the course / user id off the first
        const { courseId, userId } = memberships[0];
        tutorialGroupMemberAdapter.setOne(state, {
          courseId,
          userId,
          memberships,
        });
      }
    },
  },
});
