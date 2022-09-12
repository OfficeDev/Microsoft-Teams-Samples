import { Course } from 'models';
import { CourseService } from '../CourseService';
import { createTimedCacheFunction } from 'cache/createTimedCacheFunction';
import { constant } from 'lodash';

export class CourseServiceTimedCacheDecorator implements CourseService {
  constructor(decorated: CourseService, cacheTimeMs: number) {
    this.loadCourses = createTimedCacheFunction(
      cacheTimeMs,
      constant(0),
      decorated.loadCourses.bind(decorated),
    );
    this.updateCourse = decorated.updateCourse.bind(decorated);
  }
  loadCourses: () => Promise<Course[]>;
  updateCourse: (course: Course) => Promise<Course>;
}
