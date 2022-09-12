import { Answer } from 'models';
import { AnswerService } from '../AnswerService';
import { createTimedCacheFunction } from 'cache/createTimedCacheFunction';

export class AnswerServiceTimedCacheDecorator implements AnswerService {
  private decorated: AnswerService;

  constructor(decorated: AnswerService, cacheTimeMs: number) {
    this.decorated = decorated;
    this.postAnswer = this.decorated.postAnswer.bind(this.decorated);
    this.updateAnswer = this.decorated.updateAnswer.bind(this.decorated);

    this.loadAnswers = createTimedCacheFunction(
      cacheTimeMs,
      (courseId: string, channelId: string, questionId: string) =>
        `${courseId}-${channelId}-${questionId}`,
      this.decorated.loadAnswers.bind(this.decorated),
    );
    this.loadAnswer = createTimedCacheFunction(
      cacheTimeMs,
      (
        courseId: string,
        channelId: string,
        questionId: string,
        answerId: string,
      ) => `${courseId}-${channelId}-${questionId}-${answerId}`,
      this.decorated.loadAnswer.bind(this.decorated),
    );
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
