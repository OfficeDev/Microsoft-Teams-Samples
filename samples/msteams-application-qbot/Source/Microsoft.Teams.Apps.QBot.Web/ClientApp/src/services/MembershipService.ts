import { User, CourseMember, TutorialGroupMember } from 'models';

export interface MembershipService {
  loadMembersForCourse(
    courseId: string,
  ): Promise<{
    users: User[];
    courseMemberships: CourseMember[];
    tutorialGroupMemberships: TutorialGroupMember[];
  }>;
  assignMembership(
    user: User,
    member: CourseMember,
    tutorialGroupMembers: TutorialGroupMember[],
  ): Promise<void>;
}
