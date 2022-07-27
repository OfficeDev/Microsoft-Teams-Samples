import { AnswerService } from './AnswerService';
import { ChannelService } from './ChannelService';
import { MembershipService } from './MembershipService';
import { CourseService } from './CourseService';
import { KnowledgeBaseService } from './KnowledgeBaseService';
import { QuestionService } from './QuestionService';
import { TutorialGroupService } from './TutorialGroupsService';
import { UserService } from './UserService';

export type QBotService = AnswerService &
  MembershipService &
  ChannelService &
  CourseService &
  QuestionService &
  TutorialGroupService &
  KnowledgeBaseService &
  UserService;

export interface CreateQbotServiceOptions {
  answerService: AnswerService;
  channelService: ChannelService;
  courseMemberService: MembershipService;
  courseService: CourseService;
  knowledgeBaseService: KnowledgeBaseService;
  questionService: QuestionService;
  tutorialGroupService: TutorialGroupService;
  userService: UserService;
}

/**
 * Utitlity for composing the individual parts of the QBot service into
 * a single QBot service
 * @returns A composed QBot service
 */
export function createQBotService({
  answerService,
  channelService,
  courseMemberService,
  courseService,
  knowledgeBaseService,
  questionService,
  tutorialGroupService,
  userService,
}: CreateQbotServiceOptions): QBotService {
  return {
    // Answer Service
    loadAnswer: answerService.loadAnswer.bind(answerService),
    loadAnswers: answerService.loadAnswers.bind(answerService),
    postAnswer: answerService.postAnswer.bind(answerService),
    updateAnswer: answerService.updateAnswer.bind(answerService),

    // Channel Service
    loadChannelsForCourse: channelService.loadChannelsForCourse.bind(
      channelService,
    ),

    // Course Members Service
    loadMembersForCourse: courseMemberService.loadMembersForCourse.bind(
      courseMemberService,
    ),
    assignMembership: courseMemberService.assignMembership.bind(
      courseMemberService,
    ),

    // Course Service
    loadCourses: courseService.loadCourses.bind(courseService),
    updateCourse: courseService.updateCourse.bind(courseService),

    // Question service
    loadQuestions: questionService.loadQuestions.bind(questionService),
    updateQuestion: questionService.updateQuestion.bind(questionService),
    loadAllCourseQuestions: questionService.loadAllCourseQuestions.bind(
      questionService,
    ),

    // TutorialGroup Service
    loadTutorialGroupsForCourse: tutorialGroupService.loadTutorialGroupsForCourse.bind(
      tutorialGroupService,
    ),
    createTutorialGroup: tutorialGroupService.createTutorialGroup.bind(
      tutorialGroupService,
    ),
    deleteTutorialGroup: tutorialGroupService.deleteTutorialGroup.bind(
      tutorialGroupService,
    ),
    updateTutorialGroup: tutorialGroupService.updateTutorialGroup.bind(
      tutorialGroupService,
    ),

    // User Service
    loadUser: userService.loadUser.bind(userService),

    // KnowledgeBase Service
    loadKnowledgeBases: knowledgeBaseService.loadKnowledgeBases.bind(
      knowledgeBaseService,
    ),
    createKnowledgeBase: knowledgeBaseService.createKnowledgeBase.bind(
      knowledgeBaseService,
    ),
    deleteKnowledgeBase: knowledgeBaseService.deleteKnowledgeBase.bind(
      knowledgeBaseService,
    ),
    updateKnowledgeBase: knowledgeBaseService.updateKnowledgeBase.bind(
      knowledgeBaseService,
    ),
  };
}
