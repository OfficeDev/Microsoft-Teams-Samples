import { Question } from 'models';
import { QuestionService } from '../QuestionService';
import { createTimedCacheFunction } from 'cache/createTimedCacheFunction';
import { constant } from 'lodash';

export class QuestionServiceTimedCacheDecorator implements QuestionService {
  constructor(decorated: QuestionService, cacheTimeMs: number) {
    this.loadQuestions = createTimedCacheFunction(
      cacheTimeMs,
      constant(0),
      decorated.loadQuestions.bind(decorated),
    );
    this.updateQuestion = decorated.updateQuestion.bind(decorated);
    this.loadAllCourseQuestions = createTimedCacheFunction(
      cacheTimeMs,
      (courseId) => courseId,
      decorated.loadAllCourseQuestions.bind(decorated),
    );
  }

  loadQuestions: () => Promise<Question[]>;
  updateQuestion: (question: Question) => Promise<Question>;
  loadAllCourseQuestions: (courseId: string) => Promise<Question[]>;
}
