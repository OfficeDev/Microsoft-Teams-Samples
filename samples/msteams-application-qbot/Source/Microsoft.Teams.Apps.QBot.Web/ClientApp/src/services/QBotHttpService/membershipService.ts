import {
  CourseMember,
  CourseMemberRole,
  TutorialGroupMember,
  User,
} from 'models';
import { MembershipService } from 'services/MembershipService';
import { Request } from 'services/requestBuilder';

interface CourseMemberDTO {
  teamId: string;
  aadId: string;
  name: string;
  profilePicUri?: string;
  role: CourseMemberRole;
  upn: string;
  tutorialGroupMembership: string[];
}
function castDto(dto: CourseMemberDTO, { courseId }: { courseId: string }) {
  const user: User = {
    aadId: dto.aadId,
    id: dto.aadId,
    name: dto.name,
    upn: dto.upn,
  };
  const member: CourseMember = {
    courseId,
    role: dto.role,
    userId: dto.aadId,
  };
  const tutorialGroupMemberships: TutorialGroupMember[] = dto.tutorialGroupMembership.map(
    (tutorialGroupId) => ({
      courseId,
      tutorialGroupId,
      userId: dto.aadId,
    }),
  );
  return { user, member, tutorialGroupMemberships };
}

function castDtos(dtos: CourseMemberDTO[], { courseId }: { courseId: string }) {
  const converted = dtos.map((dto) => castDto(dto, { courseId }));
  const users = converted.map((converted) => converted.user);
  const courseMemberships = converted.map((converted) => converted.member);
  const tutorialGroupMemberships = converted.flatMap(
    (converted) => converted.tutorialGroupMemberships,
  );

  return { users, courseMemberships, tutorialGroupMemberships };
}

export class HttpMembershipService implements MembershipService {
  private readonly request: Request;
  constructor({ request }: { request: Request }) {
    this.request = request;
  }

  async loadMembersForCourse(courseId: string) {
    return await this.request({
      url: `/api/courses/${courseId}/members`,
      dtoCast: (dtos: CourseMemberDTO[]) => castDtos(dtos, { courseId }),
    });
  }

  //TODO(nibeauli): implement this.
  async assignMembership(
    user: User,
    member: CourseMember,
    tutorialGroupMembers: TutorialGroupMember[],
  ) {
    await this.request({
      url: `/api/courses/${member.courseId}/members/${user.aadId}`,
      method: 'PUT',
      body: {
        aadId: user.aadId,
        role: member.role,
        tutorialGroupMembership: tutorialGroupMembers.map(
          (tg) => tg.tutorialGroupId,
        ),
      },
    });
  }
}
