import { TutorialGroupService } from '../TutorialGroupsService';
import { createTimedCacheFunction } from 'cache/createTimedCacheFunction';

export class TutorialGroupServiceTimedCacheDecorator
  implements TutorialGroupService {
  constructor(decorated: TutorialGroupService, cacheTimeMs: number) {
    this.loadTutorialGroupsForCourse = createTimedCacheFunction(
      cacheTimeMs,
      (courseId) => courseId,
      decorated.loadTutorialGroupsForCourse.bind(decorated),
    );

    this.createTutorialGroup = decorated.createTutorialGroup.bind(decorated);
    this.updateTutorialGroup = decorated.updateTutorialGroup.bind(decorated);
    this.deleteTutorialGroup = decorated.deleteTutorialGroup.bind(decorated);
  }

  updateTutorialGroup: TutorialGroupService['updateTutorialGroup'];
  deleteTutorialGroup: TutorialGroupService['deleteTutorialGroup'];
  loadTutorialGroupsForCourse: TutorialGroupService['loadTutorialGroupsForCourse'];
  createTutorialGroup: TutorialGroupService['createTutorialGroup'];
}
