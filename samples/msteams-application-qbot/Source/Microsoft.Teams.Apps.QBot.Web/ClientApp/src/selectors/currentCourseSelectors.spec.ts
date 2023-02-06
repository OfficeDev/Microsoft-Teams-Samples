import { createLocation } from 'history';
import { QBotState } from 'compositionRoot';
import fakeState from './fakeState.json';
import {
  selectCurrentCourseLecturers,
  selectCurrentCourseMembers,
  selectCurrentCourseMemberUsers,
  selectCurrentCourseNonLecturers,
  selectCurrentPathCourse,
  selectCurrentPathCourseIndex,
  selectPathCourseId,
} from './currentCourseSelectors';

const baseState = (fakeState as unknown) as QBotState;

describe('selectPathCourseId', () => {
  it('should select a courseId from the path in personal context', () => {
    const state = {
      ...baseState,
      router: {
        location: createLocation('/personal/courses/courseId1'),
        action: 'PUSH',
      },
    } as QBotState;
    const courseId = selectPathCourseId(state);
    expect(courseId).toEqual('courseId1');
  });

  it('should select a courseId from the path in teams context', () => {
    const state = {
      ...baseState,
      router: {
        location: createLocation('/team/courses/courseId1'),
        action: 'PUSH',
      },
    } as QBotState;
    const courseId = selectPathCourseId(state);
    expect(courseId).toEqual('courseId1');
  });

  it('should return undefined when there is no courseId parameter', () => {
    const state = {
      router: {
        location: createLocation('/team/notcourse/notcourseid'),
        action: 'PUSH',
      },
    } as QBotState;
    const courseId = selectPathCourseId(state);
    expect(courseId).toBeUndefined();
  });
});

describe('selectCurrentPathCourse', () => {
  it('should get the the course specified in the path', () => {
    const course = baseState.courses.courses[0];
    const state = {
      ...baseState,
      router: {
        location: createLocation(`/personal/courses/${course.teamAadObjectId}`),
        action: 'PUSH',
      },
    } as QBotState;
    const currentCourse = selectCurrentPathCourse(state);
    expect(currentCourse).toEqual(course);
  });

  it('should get undefined when the path is not for a course', () => {
    const state = {
      ...baseState,
      router: {
        location: createLocation('/personal/not-a-course-path'),
        action: 'PUSH',
      },
    } as QBotState;
    const currentCourse = selectCurrentPathCourse(state);
    expect(currentCourse).toBeUndefined();
  });
});

describe('selectCurrentPathCourseIndex', () => {
  it('should get the index in the course list of the course in the path', () => {
    const course = baseState.courses.courses[0];
    const state = {
      ...baseState,
      router: {
        location: createLocation(`/personal/courses/${course.teamAadObjectId}`),
        action: 'PUSH',
      },
    } as QBotState;
    const currentCourseIndex = selectCurrentPathCourseIndex(state);
    expect(currentCourseIndex).toEqual(0);
  });
});

describe('selectCurrentCourseMembers', () => {
  it('should get membership objects for this course', () => {
    const course = baseState.courses.courses[0];
    const member = baseState.courseMembers.members[0];
    const notMember = baseState.courseMembers.members[4];
    const state = {
      ...baseState,
      router: {
        location: createLocation(`/personal/courses/${course.teamAadObjectId}`),
        action: 'PUSH',
      },
    };
    const selectedMembers = selectCurrentCourseMembers(state);
    expect(selectedMembers).toHaveLength(4);
    expect(selectedMembers).toContain(member);
    expect(selectedMembers).not.toContain(notMember);
  });
});

describe('selectCurrentCourseMemberUsers', () => {
  it('should all the users who are a member of this course', () => {
    const course = baseState.courses.courses[0];
    const user = baseState.users.users[1];
    const state = {
      ...baseState,
      router: {
        location: createLocation(`/personal/courses/${course.teamAadObjectId}`),
        action: 'PUSH',
      },
    };
    const selectedUsers = selectCurrentCourseMemberUsers(state);
    expect(selectedUsers).toHaveLength(4);
    expect(selectedUsers).toContain(user);
  });
});

describe('selectCurrentCourseLecturers', () => {
  it('should all the users who are a lecturer in the course', () => {
    const course = baseState.courses.courses[0];
    const state = {
      ...baseState,
      router: {
        location: createLocation(`/personal/courses/${course.teamAadObjectId}`),
        action: 'PUSH',
      },
    };
    const selectedUsers = selectCurrentCourseLecturers(state);
    expect(selectedUsers).toHaveLength(1);
    expect(selectedUsers[0].id).toEqual('b711e5b7-fd2e-4d02-8921-cddc692601fb');
  });
});

describe('selectCurrentCourseNonLecturers', () => {
  it('should all the users who are a lecturer in the course', () => {
    const course = baseState.courses.courses[0];
    const state = {
      ...baseState,
      router: {
        location: createLocation(`/personal/courses/${course.teamAadObjectId}`),
        action: 'PUSH',
      },
    };
    const selectedUsers = selectCurrentCourseNonLecturers(state);
    expect(selectedUsers).toHaveLength(3);
    // we shouldn't see the user who was a lecturer
    expect(selectedUsers.map((u) => u.id)).not.toContain(
      'b711e5b7-fd2e-4d02-8921-cddc692601fb',
    );
  });
});
