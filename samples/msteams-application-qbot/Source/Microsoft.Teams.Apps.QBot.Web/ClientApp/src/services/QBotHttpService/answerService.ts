import { Answer } from 'models';
import { AnswerService } from 'services/AnswerService';
import { Request } from 'services/requestBuilder';
import { get as getIn, omit } from 'lodash';

interface ResponseDTO {
  id: string;
  questionId: string;
  messageId: string;
  message: string;
  authorId: string;
  timeStamp: Date;
}

function castDto(
  dto: ResponseDTO,
  {
    channelId,
    courseId,
    botName,
  }: { channelId: string; courseId: string; botName: string },
): Answer {
  return {
    authorId: dto.authorId,
    id: dto.id,
    message: extractAnswerText({ message: dto.message, botName }),
    messageId: dto.messageId,
    questionId: dto.questionId,
    channelId,
    courseId,
    acceptedById: undefined,
  };
}

export function unformatMessage({
  message,
  botName,
}: {
  message: string;
  botName: string;
}) {
  const div = document.createElement('div');
  div.innerHTML = message;
  for (const mention of div.querySelectorAll('at')) {
    if ((mention.textContent?.indexOf(botName) ?? -1) >= 0) {
      mention.parentElement?.removeChild(mention);
    }
  }
  return div.textContent ?? '';
}

export function extractAnswerText({
  message,
  botName,
}: {
  message: string;
  botName: string;
}) {
  if (!message) return '';
  if (message.startsWith('{')) {
    try {
      return getIn<string>(JSON.parse(message), ['body', 0, 'text'], '');
    } catch (e) {
      throw new Error(
        `Failed to parse adaptive card json for answer:\n ${message}`,
      );
    }
  }
  return unformatMessage({ message, botName });
}

export class HttpAnswerService implements AnswerService {
  private readonly request: Request;
  private readonly botName: string;

  constructor({ request, botName }: { request: Request; botName: string }) {
    this.request = request;
    this.botName = botName;
  }
  async loadAnswer(
    courseId: string,
    channelId: string,
    questionId: string,
    answerId: string,
  ): Promise<Answer> {
    return this.request({
      url: `/api/Courses/${courseId}/channels/${channelId}/questions/${questionId}/responses/${answerId}`,
      dtoCast: (dto: ResponseDTO) =>
        castDto(dto, { channelId, courseId, botName: this.botName }),
    });
  }

  async loadAnswers(
    courseId: string,
    channelId: string,
    questionId: string,
  ): Promise<Answer[]> {
    return this.request({
      url: `/api/courses/${courseId}/channels/${channelId}/questions/${questionId}/responses`,
      dtoCast: (responses: ResponseDTO[]) =>
        responses.map((dto) =>
          castDto(dto, { channelId, courseId, botName: this.botName }),
        ),
    });
  }

  async postAnswer(
    courseId: string,
    channelId: string,
    questionId: string,
    answer: Answer,
  ): Promise<Answer> {
    const answerDTO = {
      ...omit(answer, 'id', 'messageId'),
      courseId,
      channelId,
    };
    const url = `/api/courses/${courseId}/channels/${channelId}/questions/${questionId}/answer`;
    return await this.request({
      url,
      method: 'POST',
      body: answerDTO,
      // TODO(nibeauli): this is probably not correct, the API is probably returning a DTO
      dtoCast: (answer: Answer) => answer,
    });
  }

  updateAnswer(answer: Answer): Promise<Answer> {
    throw new Error('Method not implemented.');
  }
}
