import { QBotService, createQBotService } from 'services/QBotService';
import {
  bearerTokenMiddleware,
  composeFetchMiddleware,
  defaultHeaders,
  throwOnFailureMiddleware,
  requestBuilder,
} from 'services/requestBuilder';
import { ITeamsService } from 'services/TeamsService';
import { HttpAnswerService } from './answerService';
import { HttpChannelService } from './channelService';
import { HttpMembershipService } from './membershipService';
import { HttpCourseService } from './courseService';
import { HttpKnowledgeBaseService } from './knowledgeBaseService';
import { HttpQuestionService } from './questionService';
import { HttpUserService } from './userService';
import { HttpTutorialGroupService } from './tutorialGroupService';

const DefaultHeaders: HeadersInit = {
  Accept: 'application/json, text/plain',
  'Content-Type': 'application/json;charset=UTF-8',
};

export interface QBotHttpServiceOptions {
  teamsService: ITeamsService;
  botName: string;
}
export function createQBotHttpService({
  teamsService,
  botName,
}: QBotHttpServiceOptions): QBotService {
  const customFetch = composeFetchMiddleware([
    defaultHeaders(DefaultHeaders),
    bearerTokenMiddleware(() => teamsService.getAuthToken()),
    throwOnFailureMiddleware(),
  ])(fetch);
  const request = requestBuilder(customFetch);
  const answerService = new HttpAnswerService({ request, botName });
  const channelService = new HttpChannelService({ request });
  const courseMemberService = new HttpMembershipService({ request });
  const courseService = new HttpCourseService({ request, teamsService });
  const knowledgeBaseService = new HttpKnowledgeBaseService({
    request,
    teamsService,
  });
  const questionService = new HttpQuestionService({
    request,
    channelService,
    botName,
  });
  const tutorialGroupService = new HttpTutorialGroupService({ request });
  const userService = new HttpUserService({ request });

  return createQBotService({
    answerService,
    channelService,
    courseMemberService,
    courseService,
    knowledgeBaseService,
    questionService,
    tutorialGroupService,
    userService,
  });
}
