import { createSingleEntrantFunction } from 'cache/createSingleEntrantFunction';
import { MembershipService } from 'services/MembershipService';

/**
 * CourseMemberService decorator for single-entrancy on the read operations.
 */
export class MembershipServiceSingleEntrantDecorator
  implements MembershipService {
  constructor(decorated: MembershipService) {
    this.loadMembersForCourse = createSingleEntrantFunction(
      (courseId: string) => courseId,
      decorated.loadMembersForCourse.bind(decorated),
    );

    this.assignMembership = decorated.assignMembership.bind(decorated);
  }

  loadMembersForCourse: MembershipService['loadMembersForCourse'];

  assignMembership: MembershipService['assignMembership'];
}
