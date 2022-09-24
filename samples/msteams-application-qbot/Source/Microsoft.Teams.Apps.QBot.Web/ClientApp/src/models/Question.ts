import { Answer } from './Answer';
import { User } from './User';

export interface Question {
  id: string;
  courseId: string;
  authorId: string;
  messageText: string;
  channelId: string;
  messageId: string;
  answerId?: string;
  qnAPairId: string;
  timeStamp: Date;
}

export interface FullQuestion extends Question {
  channelName?: string;
  courseName?: string;
  answer?: Answer;
  user: User;
}
