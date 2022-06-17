import { Course } from 'models';
import { CourseService } from 'services/CourseService';
import { ITeamsService } from 'services/TeamsService';
import { Request } from 'services/requestBuilder';

interface CourseDTO {
  id: string;
  teamId: string;
  teamAadObjectId: string;
  name: string;
  profilePicUri?: string;
  hasMultipleTutorialGroups: boolean;
  defaultTutorialGroupId: string;
  knowledgeBaseId?: string;
}
function castDto(dto: CourseDTO): Course {
  return {
    id: dto.id,
    displayName: dto.name,
    defaultTutorialGroupId: dto.defaultTutorialGroupId,
    hasMultipleTutorialGroups: dto.hasMultipleTutorialGroups,
    // QBot returns the images as base64 encoded strings, we convert them to data uris
    iconUrl: `data:image/png;base64, ${dto.profilePicUri || ''}`,
    teamAadObjectId: dto.teamAadObjectId,
    teamId: dto.teamId,
    name: dto.name,
    knowledgeBaseId: dto.knowledgeBaseId,
  };
}

export class HttpCourseService implements CourseService {
  private readonly request: Request;
  private readonly teamsService: ITeamsService;
  constructor({
    request,
    teamsService,
  }: {
    request: Request;
    teamsService: ITeamsService;
  }) {
    this.request = request;
    this.teamsService = teamsService;
  }

  async loadCourses(): Promise<Course[]> {
    // TODO(nibeauli): The following should probably be done at the middleware level to reduce cross-coupling
    const teamsContext = await this.teamsService.getContext();
    const userId = teamsContext.userObjectId;
    return await this.request({
      url: `/api/users/${userId}/courses`,
      dtoCast: (courseDtos: CourseDTO[]) =>
        courseDtos.map((course) => castDto(course)),
    });
  }

  async updateCourse(course: Course): Promise<Course> {
    const courseDTO: CourseDTO = {
      id: course.id,
      name: course.displayName,
      defaultTutorialGroupId: course.defaultTutorialGroupId,
      hasMultipleTutorialGroups: course.hasMultipleTutorialGroups,
      teamAadObjectId: course.teamAadObjectId,
      teamId: course.teamId,
      knowledgeBaseId: course.knowledgeBaseId,
    };
    await this.request({
      url: `/api/courses/${course.id}`,
      method: 'PUT',
      body: courseDTO,
    });
    return course;
  }
}
