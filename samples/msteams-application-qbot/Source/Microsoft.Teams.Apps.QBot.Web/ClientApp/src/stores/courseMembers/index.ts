import {
  createEntityAdapter,
  createSlice,
  PayloadAction,
} from '@reduxjs/toolkit';
import {
  createCommandHandler,
  iterableCommandHandler,
} from 'createCommandHandler';
import { CourseMember, TutorialGroupMember, User } from 'models';
import { MembershipService } from 'services/MembershipService';
import { tutorialGroupMemberSlice } from 'stores/tutorialGroupMembers';
import { userSlice } from '../users';
import { globalErrorSlice } from 'stores/globalError';

export const courseMemberAdapter = createEntityAdapter<CourseMember>({
  selectId: (member) => `${member.courseId}-${member.userId}`,
  // No logical natural ordering, just order by the user id.
  sortComparer: (a, b) => a.userId.localeCompare(b.userId),
});

const initialState = courseMemberAdapter.getInitialState({
  loading: false,
});
export type CourseMemberState = typeof initialState;

export const courseMemberSlice = createSlice({
  name: 'courseMembers',
  initialState,
  reducers: {
    courseMembersLoading(state) {
      state.loading = true;
    },
    courseMembersLoaded(
      state,
      event: PayloadAction<{ members: CourseMember[] }>,
    ) {
      state.loading = false;
      courseMemberAdapter.upsertMany(state, event.payload.members);
    },
  },
});

export function createCourseMemberHandlers({
  courseMembersService,
}: {
  courseMembersService: MembershipService;
}) {
  async function* loadCourseMembers(
    command: PayloadAction<{ courseId: string }>,
  ) {
    try {
      yield userSlice.actions.usersLoading();
      yield courseMemberSlice.actions.courseMembersLoading();
      yield tutorialGroupMemberSlice.actions.tutorialGroupMembersLoading();
      const {
        users,
        courseMemberships,
        tutorialGroupMemberships,
      } = await courseMembersService.loadMembersForCourse(
        command.payload.courseId,
      );
      yield userSlice.actions.usersLoaded({ users });
      yield courseMemberSlice.actions.courseMembersLoaded({
        members: courseMemberships,
      });
      yield tutorialGroupMemberSlice.actions.tutorialGroupMembersLoaded({
        members: tutorialGroupMemberships,
      });
    } catch (error) {
      yield globalErrorSlice.actions.showError({
        error: error instanceof Error ? error : new Error(`${error}`),
      });
    }
  }

  async function* assignRoles(
    command: PayloadAction<{
      members: {
        user: User;
        courseMembership: CourseMember;
        tutorialGroupMemberships: TutorialGroupMember[];
      }[];
    }>,
  ) {
    try {
      const promises = command.payload.members.map(
        ({ user, courseMembership, tutorialGroupMemberships }) =>
          courseMembersService.assignMembership(
            user,
            courseMembership,
            tutorialGroupMemberships,
          ),
      );
      await Promise.all(promises);
      const courseMemberships = command.payload.members.map(
        (m) => m.courseMembership,
      );
      const tutorialGroupMemberships = command.payload.members.flatMap(
        (m) => m.tutorialGroupMemberships,
      );
      yield courseMemberSlice.actions.courseMembersLoaded({
        members: courseMemberships,
      });
      yield tutorialGroupMemberSlice.actions.tutorialGroupMembersLoaded({
        members: tutorialGroupMemberships,
      });
    } catch (error) {
      yield globalErrorSlice.actions.showError({
        error: error instanceof Error ? error : new Error(`${error}`),
      });
    }
  }
  return createCommandHandler({
    name: 'courseMembers',
    handlers: {
      loadCourseMembers: iterableCommandHandler(loadCourseMembers),
      assignRoles: iterableCommandHandler(assignRoles),
    },
  });
}
export type CourseMemberHandler = ReturnType<typeof createCourseMemberHandlers>;
