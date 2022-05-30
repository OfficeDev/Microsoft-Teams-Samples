import { Answer } from 'models';

export interface AnswerService {
  loadAnswer(
    courseId: string,
    channelId: string,
    questionId: string,
    answerId: string,
  ): Promise<Answer>;
  loadAnswers(
    courseId: string,
    channelId: string,
    questionId: string,
  ): Promise<Answer[]>;
  postAnswer(
    courseId: string,
    channelId: string,
    questionId: string,
    answer: Answer,
  ): Promise<Answer>;
  updateAnswer(answer: Answer): Promise<Answer>;
}
