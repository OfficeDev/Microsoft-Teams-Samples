import { MembershipService } from '../MembershipService';
import { createTimedCacheFunction } from 'cache/createTimedCacheFunction';

export class MembershipServiceTimedCacheDecorator implements MembershipService {
  private decorated: MembershipService;

  constructor(decorated: MembershipService, cacheTimeMs: number) {
    this.decorated = decorated;
    this.loadMembersForCourse = createTimedCacheFunction(
      cacheTimeMs,
      (courseId) => courseId,
      this.decorated.loadMembersForCourse.bind(this.decorated),
    );
    this.assignMembership = this.decorated.assignMembership.bind(
      this.decorated,
    );
  }

  loadMembersForCourse: MembershipService['loadMembersForCourse'];

  assignMembership: MembershipService['assignMembership'];
}
