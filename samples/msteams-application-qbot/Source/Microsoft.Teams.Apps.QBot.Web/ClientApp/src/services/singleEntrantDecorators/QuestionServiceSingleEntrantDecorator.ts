import { createSingleEntrantFunction } from 'cache/createSingleEntrantFunction';
import { Question } from 'models';
import { QuestionService } from 'services/QuestionService';
import { constant } from 'lodash';

/**
 * QuestionService decorator for single-entrancy on the read operations.
 */
export class QuestionServiceSingleEntrantDecorator implements QuestionService {
  constructor(decorated: QuestionService) {
    this.updateQuestion = decorated.updateQuestion.bind(decorated);
    this.loadAllCourseQuestions = createSingleEntrantFunction(
      (courseId: string) => courseId,
      decorated.loadAllCourseQuestions.bind(decorated),
    );
    this.loadQuestions = createSingleEntrantFunction(
      constant(0),
      decorated.loadQuestions.bind(decorated),
    );
  }

  loadQuestions: () => Promise<Question[]>;

  loadAllCourseQuestions: (courseId: string) => Promise<Question[]>;

  updateQuestion: (question: Question) => Promise<Question>;
}
