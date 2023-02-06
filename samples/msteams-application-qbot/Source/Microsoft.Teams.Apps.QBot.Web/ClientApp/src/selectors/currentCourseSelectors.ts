import { createSelector } from 'reselect';
import { flow, memoize } from 'lodash';
import { createMatchSelector } from 'connected-react-router';
import { QBotState } from 'compositionRoot';
import { User } from 'models';
import { selectTutorialGroups } from './tutorialGroupSelectors';
import { selectCourses } from './courseSelectors';
import { selectUsersById } from './userSelectors';
import { selectCourseMemberships } from './courseMembersSelectors';
import { selectUserCourseMembershipsByCourseId } from './userMembershipSelectors';
import { selectChannels } from './channelsSelectors';

export const selectPathCourseId = flow(
  createMatchSelector<QBotState, { courseId?: string }>(
    '/:context/courses/:courseId',
  ),
  memoize((match) => match?.params.courseId),
);

export const selectCourseIdFromUrl = flow(
  createMatchSelector<QBotState, { courseId?: string }>(
    '/:context/course/:courseId',
  ),
  memoize((match) => match?.params.courseId),
);

export const selectCurrentPathCourse = createSelector(
  selectCourses,
  selectPathCourseId,
  (courses, courseId) => {
    if (courseId === undefined) return undefined;
    return courses.find((c) => c.teamAadObjectId === courseId);
  },
);

export const selectCurrentPathCourseId = createSelector(
  selectCurrentPathCourse,
  (currentCourse) => currentCourse?.id,
);

export const selectCurrentPathCourseById = createSelector(
  selectCourses,
  selectPathCourseId,
  (courses, courseId) => {
    if (courseId === undefined) return undefined;
    return courses.find((c) => c.id === courseId);
  },
);

export const selectCurrentPathCourseIndex = createSelector(
  selectPathCourseId,
  selectCourses,
  (currentcourseId, allCourses) => {
    return allCourses.findIndex(
      (course) => course?.teamAadObjectId === currentcourseId,
    );
  },
);

export const selectCurrentCourseMembers = createSelector(
  selectCurrentPathCourse,
  selectCourseMemberships,
  (course, members) => {
    return members.filter((member) => member.courseId === course?.id);
  },
);

export const selectCurrentCourseMemberUsers = createSelector(
  selectCurrentCourseMembers,
  selectUsersById,
  (members, usersById) => {
    return members
      .map((member) => usersById.get(member.userId))
      .filter((user) => user !== undefined) as User[];
  },
);

export const selectCurrentCourseLecturers = createSelector(
  selectCurrentPathCourse,
  selectCurrentCourseMembers,
  selectUsersById,
  (course, members, usersById) => {
    return members
      .filter((member) => member.role === 'Educator')
      .map((member) => usersById.get(member.userId))
      .filter((user) => user !== undefined) as User[];
  },
);

export const selectCurrentCourseNonLecturers = createSelector(
  selectCurrentCourseLecturers,
  selectCurrentCourseMemberUsers,
  (lecturers, users) => {
    const lecturerIdSet = new Set(lecturers.map((l) => l.id));
    return users.filter((user) => !lecturerIdSet.has(user.id));
  },
);

export const selectCurrentCourseTutorialGroups = createSelector(
  selectCurrentPathCourse,
  selectTutorialGroups,
  (currentCourse, tutorialGroups) =>
    tutorialGroups.filter((tg) => tg.courseId === currentCourse?.id),
);

export const selectCurrentCourseUserMemberships = createSelector(
  selectCurrentPathCourse,
  selectUserCourseMembershipsByCourseId,
  (course, userCourseMembershipsByCourseId) =>
    course?.id ? userCourseMembershipsByCourseId.get(course.id) ?? [] : [],
);

export const selectCurrentCourseChannels = createSelector(
  selectCurrentPathCourse,
  selectChannels,
  (course, channels) =>
    course === undefined
      ? []
      : channels.filter((channel) => channel.courseId === course.id),
);
