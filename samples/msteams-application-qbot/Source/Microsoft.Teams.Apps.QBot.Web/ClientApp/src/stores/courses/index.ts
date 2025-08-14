import { Course } from 'models';
import {
  createSlice,
  createEntityAdapter,
  PayloadAction,
} from '@reduxjs/toolkit';

import {
  asyncCommandHandler,
  createCommandHandler,
  iterableCommandHandler,
} from 'createCommandHandler';
import { CourseService } from 'services/CourseService';
import { globalErrorSlice } from 'stores/globalError';

const courseAdapter = createEntityAdapter<Course>({
  selectId: (course) => course.id,
  sortComparer: (a, b) => a.displayName.localeCompare(b.displayName),
});

const initialState = courseAdapter.getInitialState({
  loading: false,
  loaded: false,
});
const courseSlice = createSlice({
  name: 'courses',
  initialState,
  reducers: {
    setCourse: (state, action: PayloadAction<{ course: Course }>) => {
      courseAdapter.setOne(state, action.payload.course);
    },
    coursesLoading: (state) => {
      state.loading = true;
    },
    coursesLoaded(state, action: PayloadAction<{ courses: Course[] }>) {
      state.loading = false;
      state.loaded = true;
      courseAdapter.upsertMany(state, action.payload.courses);
    },
  },
});

function createCourseHandlers({
  courseService,
}: {
  courseService: CourseService;
}) {
  async function* loadCourses(command: PayloadAction<void>) {
    yield courseSlice.actions.coursesLoading();
    try {
      const courses = await courseService.loadCourses();
      yield courseSlice.actions.coursesLoaded({ courses });
    } catch (error) {
      console.warn('Failed to load courses', { error });
      yield globalErrorSlice.actions.showError({
        error: error instanceof Error ? error : new Error(`${error}`),
      });
      yield courseSlice.actions.coursesLoaded({ courses: [] });
    }
  }
  async function setCourse({
    payload: { course },
  }: PayloadAction<{ course: Course }>) {
    const responseCourse = await courseService.updateCourse(course);
    return courseSlice.actions.setCourse({ course: responseCourse });
  }
  return createCommandHandler({
    name: 'courseCommands',
    handlers: {
      loadCourses: iterableCommandHandler(loadCourses),
      setCourse: asyncCommandHandler(setCourse),
    },
  });
}

export type CourseHandler = ReturnType<typeof createCourseHandlers>;

type CourseState = typeof initialState;
export { courseSlice, createCourseHandlers, courseAdapter };
export type { CourseState };
