export interface Answer {
  id: string;
  questionId: string;
  authorId: string;
  acceptedById?: string;
  channelId: string;
  messageId: string;
  courseId: string;
  message: string;
}
