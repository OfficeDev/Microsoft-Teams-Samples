import { Course } from 'models';

export interface CourseService {
  loadCourses(): Promise<Course[]>;
  updateCourse(course: Course): Promise<Course>;
}
