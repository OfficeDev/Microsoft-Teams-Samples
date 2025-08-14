import { Course } from 'models';
import { QBotState } from 'compositionRoot';
import { selectCourses, selectCoursesById } from './courseSelectors';

const course1: Course = {
  id: 'courseId1',
  displayName: 'displayName',
  teamId: 'teamId',
  teamAadObjectId: 'teamAadObjectId',
  hasMultipleTutorialGroups: false,
  defaultTutorialGroupId: 'defaultTutorialGroupId',
  iconUrl: 'https://iconUrl.png',
};

const course2: Course = {
  id: 'courseId2',
  displayName: 'displayName2',
  teamId: 'teamId2',
  teamAadObjectId: 'teamAadObjectId2',
  hasMultipleTutorialGroups: false,
  defaultTutorialGroupId: 'defaultTutorialGroupId2',
  iconUrl: 'https://iconUrl2.png',
};

describe('selectCourses', () => {
  it('should select the courses array from the state object', () => {
    const state = {
      courses: {
        courses: [course1],
      },
    } as QBotState;
    const courses = selectCourses(state);
    expect(courses).toEqual([course1]);
  });

  it('should maintain the ordering of the courses in state', () => {
    const state = {
      courses: {
        courses: [course2, course1],
      },
    } as QBotState;
    const courses = selectCourses(state);
    expect(courses).toEqual([course2, course1]);
  });
});

describe('selectCoursesById', () => {
  it('should select the courses array from the state object', () => {
    const state = {
      courses: {
        courses: [course1, course2],
      },
    } as QBotState;
    const coursesById = selectCoursesById(state);
    expect(coursesById).toEqual(
      new Map([
        [course1.id, course1],
        [course2.id, course2],
      ]),
    );
  });
});
