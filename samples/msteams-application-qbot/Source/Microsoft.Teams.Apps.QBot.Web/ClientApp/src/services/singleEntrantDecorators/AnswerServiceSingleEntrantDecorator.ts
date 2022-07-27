import { createSingleEntrantFunction } from 'cache/createSingleEntrantFunction';
import { Answer } from 'models';
import { AnswerService } from 'services/AnswerService';

/**
 * AnswerService decorator for single-entrancy on the read operations.
 */
export class AnswerServiceSingleEntrantDecorator implements AnswerService {
  constructor(decorated: AnswerService) {
    this.loadAnswers = createSingleEntrantFunction(
      (courseId: string, channelId: string, questionId: string) =>
        `${courseId}-${channelId}-${questionId}`,
      decorated.loadAnswers.bind(decorated),
    );

    this.loadAnswer = createSingleEntrantFunction(
      (
        courseId: string,
        channelId: string,
        questionId: string,
        answerId: string,
      ) => `${courseId}-${channelId}-${questionId}-${answerId}`,
      decorated.loadAnswer.bind(decorated),
    );
    this.postAnswer = decorated.postAnswer.bind(decorated);
    this.updateAnswer = decorated.updateAnswer.bind(decorated);
  }
  loadAnswer: (
    courseId: string,
    channelId: string,
    questionId: string,
    answerId: string,
  ) => Promise<Answer>;

  loadAnswers: (
    courseId: string,
    channelId: string,
    questionId: string,
  ) => Promise<Answer[]>;

  postAnswer: (
    courseId: string,
    channelId: string,
    questionId: string,
    answer: Answer,
  ) => Promise<Answer>;

  updateAnswer: (answer: Answer) => Promise<Answer>;
}
