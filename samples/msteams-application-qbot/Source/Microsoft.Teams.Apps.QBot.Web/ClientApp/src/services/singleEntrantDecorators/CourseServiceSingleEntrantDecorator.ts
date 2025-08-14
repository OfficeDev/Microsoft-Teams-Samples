import { createSingleEntrantFunction } from 'cache/createSingleEntrantFunction';
import { Course } from 'models';
import { CourseService } from 'services/CourseService';
import { constant } from 'lodash';

/**
 * CourseService decorator for single-entrancy on the read operations.
 */
export class CourseServiceSingleEntrantDecorator implements CourseService {
  constructor(decorated: CourseService) {
    this.loadCourses = createSingleEntrantFunction(
      constant(0),
      decorated.loadCourses.bind(decorated),
    );
    this.updateCourse = decorated.updateCourse.bind(decorated);
  }

  loadCourses: () => Promise<Course[]>;

  updateCourse: (course: Course) => Promise<Course>;
}
