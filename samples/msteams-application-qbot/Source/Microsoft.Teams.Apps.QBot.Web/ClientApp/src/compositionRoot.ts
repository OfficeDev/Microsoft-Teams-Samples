import { createBrowserHistory } from 'history';
import { routerMiddleware } from 'connected-react-router';
import { ApplicationInsights } from '@microsoft/applicationinsights-web';
import { createRootReducer, RootReducerParams } from './RootReducer';
import { createTutorialGroupHandlers } from './stores/tutorialGroups';
import { createCourseHandlers } from './stores/courses';
import { createQuestionHandlers } from 'stores/questions';
import { createAnswersHandlers } from 'stores/answers';
import { TeamsService } from 'services/TeamsService';
import { QBotService } from 'services/QBotService';
import { createQBotHttpService } from 'services/QBotHttpService';
import {
  AnswerServiceSingleEntrantDecorator,
  MembershipServiceSingleEntrantDecorator,
  CourseServiceSingleEntrantDecorator,
  QuestionServiceSingleEntrantDecorator,
  SingleEntrantTutorialGroupService,
} from 'services/singleEntrantDecorators';
import {
  AnswerServiceTimedCacheDecorator,
  MembershipServiceTimedCacheDecorator,
  CourseServiceTimedCacheDecorator,
  QuestionServiceTimedCacheDecorator,
  TutorialGroupServiceTimedCacheDecorator,
} from 'services/timedCacheDecorators';
import { reactPlugin } from 'appInsights';
import { TeamsServiceAIDecorator } from 'services/TeamsServiceAIDecorator';
import {
  loadMessages,
  LocaleMessages,
  SupportedLocale,
} from 'localization/localizationService';
import { FakeDataService } from 'fake/fakeDataService';
import { createUserHandlers } from 'stores/users';
import { UserServiceSingleEntrantDecorator } from 'services/singleEntrantDecorators/UserServiceSingleEntrantDecorator';
import { UserServiceTimedCacheDecorator } from 'services/timedCacheDecorators/UserServiceTimedCacheDecorator';
import { createChannelHandlers } from 'stores/channel';
import { ChannelService } from 'services/ChannelService';
import { createCourseMemberHandlers } from 'stores/courseMembers';
import { mergeCommandHandlers } from 'createCommandHandler';
import { configureStore } from '@reduxjs/toolkit';
import { createGlobalErrorHandlers } from 'stores/globalError';
import { KnowledgeBaseServiceSingleEntrantDecorator } from 'services/singleEntrantDecorators/KnowledgeBaseSingleEntrantDecorator';
import { KnowledgeBaseServiceTimedCacheDecorator } from 'services/timedCacheDecorators/KnowledgeBaseServiceTimedCacheDecorator';
import { createKnowledgeBaseHandlers } from 'stores/knowledgeBases';

const useFakeDataService =
  process.env.REACT_APP_USE_FAKE_DATA === 'true' ||
  window.location.search.indexOf('FAKE_DATA_SERVICE=true') >= 0;

export interface Configuration {
  applicationInsightsInstrumentationKey: string;
  botAppName: string;
  locale: SupportedLocale;
  messages: LocaleMessages;
}

async function loadFakeConfiguration(locale: SupportedLocale = 'en') {
  const messagesPromise = loadMessages(locale);
  const messages = await messagesPromise;
  return {
    applicationInsightsInstrumentationKey: '',
    botAppName: 'QBot (fake data)',
    locale,
    messages,
  };
}

async function loadFakeDataService(): Promise<FakeDataService> {
  const { FakeDataService } = await import('fake/fakeDataService');
  return new FakeDataService({ seed: 42 });
}

async function loadConfiguration(
  locale: SupportedLocale = 'en',
): Promise<Configuration> {
  const messagesPromise = loadMessages(locale);
  const configPromise: Promise<
    Pick<Configuration, 'applicationInsightsInstrumentationKey' | 'botAppName'>
  > = fetch('/app/config').then((r) => r.json());
  const [config, messages] = await Promise.all([
    configPromise,
    messagesPromise,
  ]);
  return {
    ...config,
    messages,
    locale,
  };
}

export async function loadCompositionRoot(
  locale: SupportedLocale,
): Promise<ReturnType<typeof compositionRoot>> {
  const configuration = await (useFakeDataService
    ? loadFakeConfiguration(locale)
    : loadConfiguration(locale));
  const fakeDataService = useFakeDataService
    ? await loadFakeDataService()
    : undefined;
  return compositionRoot(configuration, fakeDataService);
}

export function compositionRoot(
  configuration: Configuration,
  fakeDataService: QBotService | undefined = undefined,
) {
  const history = createBrowserHistory();
  const locale = configuration.locale;
  const messages = configuration.messages;
  const appInsights = new ApplicationInsights({
    config: {
      instrumentationKey: configuration.applicationInsightsInstrumentationKey,
      extensions: [reactPlugin],
      extensionConfig: {
        [reactPlugin.identifier]: {
          history,
        },
      },
      disableFetchTracking: false,
      enableAjaxPerfTracking: true,
    },
  });
  appInsights.loadAppInsights();
  const teamsService = new TeamsServiceAIDecorator(
    new TeamsService(),
    appInsights,
  );
  teamsService
    .getContext()
    .then(
      ({ userObjectId }) =>
        userObjectId && appInsights.setAuthenticatedUserContext(userObjectId),
    );

  const dataService =
    fakeDataService ??
    createQBotHttpService({ teamsService, botName: configuration.botAppName });

  // TODO(nibeauli): load this value from the configuration fetched from the server
  const cacheTimeMs = 15_000;

  const answerService = new AnswerServiceSingleEntrantDecorator(
    new AnswerServiceTimedCacheDecorator(dataService, cacheTimeMs),
  );

  //TODO(nibeauli): decorate this so it is single-entrant & caches.
  const channelService: ChannelService = dataService;

  const userService = new UserServiceSingleEntrantDecorator(
    new UserServiceTimedCacheDecorator(dataService, cacheTimeMs),
  );

  const questionService = new QuestionServiceSingleEntrantDecorator(
    new QuestionServiceTimedCacheDecorator(dataService, cacheTimeMs),
  );

  const courseService = new CourseServiceSingleEntrantDecorator(
    new CourseServiceTimedCacheDecorator(dataService, cacheTimeMs),
  );

  const tutorialGroupService = new SingleEntrantTutorialGroupService(
    new TutorialGroupServiceTimedCacheDecorator(dataService, cacheTimeMs),
  );

  const courseMembersService = new MembershipServiceSingleEntrantDecorator(
    new MembershipServiceTimedCacheDecorator(dataService, cacheTimeMs),
  );

  const knowledgeBaseService = new KnowledgeBaseServiceSingleEntrantDecorator(
    new KnowledgeBaseServiceTimedCacheDecorator(dataService, cacheTimeMs),
  );

  const { actionCreators, middleware } = mergeCommandHandlers({
    answer: createAnswersHandlers({ answerService, userService }),
    course: createCourseHandlers({ courseService }),
    user: createUserHandlers({ userService }),
    question: createQuestionHandlers({ questionService, answerService }),
    channel: createChannelHandlers({ channelService }),
    courseMember: createCourseMemberHandlers({ courseMembersService }),
    tutorialGroup: createTutorialGroupHandlers({ tutorialGroupService }),
    globalError: createGlobalErrorHandlers(),
    knowledgeBase: createKnowledgeBaseHandlers({ knowledgeBaseService }),
  });

  const initialState: RootReducerParams = {
    history,
  };

  const store = configureStore({
    reducer: createRootReducer(initialState),
    middleware: () => [routerMiddleware(history), middleware],
  });

  return {
    history,
    store,
    dataService,
    teamsService,
    locale,
    messages,
    actionCreators: actionCreators,
    configuration,
  };
}

export type QBotApplication = ReturnType<typeof compositionRoot>;
export type QBotState = ReturnType<QBotApplication['store']['getState']>;
export type QBotDispatch = QBotApplication['store']['dispatch'];
