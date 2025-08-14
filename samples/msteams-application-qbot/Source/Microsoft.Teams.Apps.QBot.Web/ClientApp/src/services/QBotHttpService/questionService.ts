import { Question } from 'models';
import { ChannelService } from 'services/ChannelService';
import { QuestionService } from 'services/QuestionService';
import { Request } from 'services/requestBuilder';
import { unformatMessage } from './answerService';

interface QuestionDTO {
  id: string;
  courseId: string;
  authorId: string;
  messageText: string;
  channelId: string;
  messageId: string;
  answerId: string | null;
  qnAPairId: string;
  timeStamp: string;
  channelName: string;
  message: string | undefined | null;
}
function castDto(dto: QuestionDTO, { botName }: { botName: string }): Question {
  return {
    id: dto.id,
    answerId: dto.answerId ? dto.answerId : undefined,
    authorId: dto.authorId,
    courseId: dto.courseId,
    channelId: dto.channelId,
    messageId: dto.messageId,
    messageText: unformatMessage({
      message: dto.message || '',
      botName,
    }),
    qnAPairId: dto.qnAPairId,
    timeStamp: new Date(dto.timeStamp),
  };
}
//TODO(nibeauli): Implement this
export class HttpQuestionService implements QuestionService {
  private readonly request: Request;
  private readonly channelService: ChannelService;
  private readonly botName: string;

  constructor({
    request,
    channelService,
    botName,
  }: {
    request: Request;
    channelService: ChannelService;
    botName: string;
  }) {
    this.request = request;
    this.channelService = channelService;
    this.botName = botName;
  }

  updateQuestion(question: Question): Promise<Question> {
    throw new Error('Method not implemented.');
  }
  async loadAllCourseQuestions(courseId: string): Promise<Question[]> {
    const channels = await this.channelService.loadChannelsForCourse(courseId);
    const questionPromises = channels.map((channel) =>
      this.loadCourseChannelQuestions(courseId, channel.id, channel.name),
    );
    const questionLists = await Promise.all(questionPromises);
    return questionLists.flatMap((x) => x);
  }

  //TODO(nibeauli): delete this method from interface & project
  loadQuestions(): Promise<Question[]> {
    throw new Error('Method not implemented.');
  }

  // TODO(nibeauli): look into making this the public interface
  // so we don't have an internal fan-out behavior
  private async loadCourseChannelQuestions(
    courseId: string,
    channelId: string,
    channelName: string,
  ): Promise<Question[]> {
    return await this.request({
      url: `/api/Courses/${courseId}/channels/${channelId}/questions`,
      dtoCast: (questionDtos: QuestionDTO[]) =>
        questionDtos.map((qdto) => castDto(qdto, { botName: this.botName })),
    });
  }
}
