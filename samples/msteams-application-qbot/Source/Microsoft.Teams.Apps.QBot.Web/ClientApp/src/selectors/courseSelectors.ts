import { flow } from 'lodash';
import { createSelector } from 'reselect';
import { QBotState } from 'compositionRoot';
import { courseAdapter } from 'stores/courses';

const courseSelectors = courseAdapter.getSelectors<QBotState>(
  (state) => state.courses,
);
export const selectCourses = courseSelectors.selectAll;

// Create a Map<id,course> from the list of all courses
// TODO(nibeauli): evaluate if we can migrate this to record format
export const selectCoursesById = flow(
  selectCourses,
  (courses) => new Map(courses.map((c) => [c.id, c])),
);

export const selectIfFirstLoadingCourses = createSelector(
  selectCourses,
  (state: QBotState) => state.courses.loading,
  (courses, isLoading) => courses.length === 0 && isLoading,
);

export const selectIfNoCourses = createSelector(
  selectCourses,
  (state: QBotState) => state.courses.loaded,
  (courses, isLoaded) => courses.length === 0 && isLoaded,
);
