import { createSingleEntrantFunction } from 'cache/createSingleEntrantFunction';
import { TutorialGroupService } from 'services/TutorialGroupsService';

/**
 * TutorialGroupService decorator for single-entrancy on the read operations.
 */
export class SingleEntrantTutorialGroupService implements TutorialGroupService {
  constructor(decorated: TutorialGroupService) {
    this.loadTutorialGroupsForCourse = createSingleEntrantFunction(
      (courseId: string) => courseId,
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
