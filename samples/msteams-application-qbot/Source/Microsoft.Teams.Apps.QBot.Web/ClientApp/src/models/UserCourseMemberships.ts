import { Course } from './Course';
import { CourseMemberRole } from './CourseMemberRole';
import { TutorialGroup } from './TutorialGroup';
import { User } from './User';

export interface UserCourseMemberships {
  user: User;
  role: CourseMemberRole;
  course: Course;
  tutorialGroups: TutorialGroup[];
}
