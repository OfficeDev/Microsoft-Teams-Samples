import { CourseMemberRole } from './CourseMemberRole';

export interface CourseMember {
  courseId: string;
  userId: string;
  role: CourseMemberRole;
}
