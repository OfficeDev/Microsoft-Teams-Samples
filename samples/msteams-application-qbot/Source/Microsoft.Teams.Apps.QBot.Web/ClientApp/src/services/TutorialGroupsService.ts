import { TutorialGroup } from 'models';

export interface TutorialGroupService {
  loadTutorialGroupsForCourse(courseId: string): Promise<TutorialGroup[]>;
  createTutorialGroup(tutorialGroup: TutorialGroup): Promise<TutorialGroup>;
  updateTutorialGroup(tutorialGroup: TutorialGroup): Promise<TutorialGroup>;
  deleteTutorialGroup(tutorialGroup: TutorialGroup): Promise<void>;
}
