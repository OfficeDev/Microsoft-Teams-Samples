import { TutorialGroup } from 'models';
import { TutorialGroupService } from 'services/TutorialGroupsService';
import { Request } from 'services/requestBuilder';

interface TutorialGroupDTO {
  id: string;
  displayName: string;
  shortCode: string;
  courseId: string;
}
function fromDto(
  dto: TutorialGroupDTO,
  { courseId }: { courseId: string },
): TutorialGroup {
  return {
    courseId,
    displayName: dto.displayName,
    id: dto.id,
    shortName: dto.shortCode,
  };
}
function toDto(model: TutorialGroup): TutorialGroupDTO {
  return {
    id: model.id,
    courseId: model.courseId,
    displayName: model.displayName,
    shortCode: model.shortName,
  };
}
export class HttpTutorialGroupService implements TutorialGroupService {
  private readonly request: Request;
  constructor({ request }: { request: Request }) {
    this.request = request;
  }
  updateTutorialGroup(tutorialGroup: TutorialGroup): Promise<TutorialGroup> {
    return this.request({
      url: `/api/TutorialGroups/${tutorialGroup.id}`,
      method: 'PUT',
      body: toDto(tutorialGroup),
      dtoCast: (dto: TutorialGroupDTO) =>
        fromDto(dto, { courseId: tutorialGroup.courseId }),
    });
  }
  deleteTutorialGroup(tutorialGroup: TutorialGroup): Promise<void> {
    return this.request({
      url: `/api/TutorialGroups/${tutorialGroup.id}`,
      method: 'DELETE',
      dtoCast: (x) => undefined,
    });
  }
  loadTutorialGroupsForCourse(courseId: string): Promise<TutorialGroup[]> {
    return this.request({
      url: `/api/Courses/${courseId}/tutorialGroups`,
      dtoCast: (dtos: TutorialGroupDTO[]) =>
        dtos.map((dto) => fromDto(dto, { courseId })),
    });
  }
  createTutorialGroup(tutorialGroup: TutorialGroup): Promise<TutorialGroup> {
    return this.request({
      url: '/api/TutorialGroups',
      method: 'POST',
      body: toDto(tutorialGroup),
      dtoCast: (dto: TutorialGroupDTO) =>
        fromDto(dto, { courseId: tutorialGroup.courseId }),
    });
  }
}
