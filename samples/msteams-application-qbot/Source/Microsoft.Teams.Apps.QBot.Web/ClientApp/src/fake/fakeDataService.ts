/* eslint-disable */
/* istanbul ignore file */
// This file is temporary during development
import * as faker from 'faker';
import { range } from 'lodash';
import {
  Answer,
  Channel,
  Course,
  CourseMember,
  CourseMemberRole,
  Question,
  TutorialGroup,
  User,
} from 'models';
import { KnowledgeBase } from 'models/KnowledgeBase';
import { TutorialGroupMember } from 'models/TutorialGroupMember';
import { QBotService } from 'services/QBotService';
import { values, groupBy } from 'lodash';

const delay = (ms: number) => new Promise((resolve) => setTimeout(resolve, ms));

function toIdMap<T>(keyFn: (obj: T) => string): (objs: T[]) => Record<string, T> {
  return function(objs: T[]) {
    const record:Record<string, T> = {};
    for (const obj of objs) {
      record[keyFn(obj)] = obj;
    }
    return record;
  }
}

const memberRoles: CourseMemberRole[] = [
  'Educator',
  'Student',
  'Tutor',
];

function generateFakeCourseData(
  course: Course,
  users: User[],
  { maxTutorialGroups }: { maxTutorialGroups?: number },
) {
  const snPrefix = faker.random.alphaNumeric(2);
  const useTutorialGroups = course.hasMultipleTutorialGroups;
  const tutorialGroups: TutorialGroup[] = useTutorialGroups
    ? range(0, 1 + faker.random.number(maxTutorialGroups ?? 3)).map((idx) => ({
        courseId: course.id,
        displayName: faker.random.words(2),
        shortName: `${snPrefix}${idx + 100}`,
        id: faker.random.uuid(),
      }))
    : [];
  const channelIds = range(0, 4).map((i) => faker.random.uuid());
  const channels: Channel[] = channelIds.map((id) => ({
    id,
    courseId: course.id,
    name: faker.company.catchPhrase(),
  }));
  const courseMembers: CourseMember[] = faker.random
    .arrayElements(users, faker.random.number({ min: 1, max: users.length})) //ensure there is at least one user
    .map((user, idx) => ({
      courseId: course.id,
      userId: user.aadId,
      // Make sure the course has at least one owner by making the first user an educator
      role: idx === 0 ? 'Educator' : faker.random.arrayElement(memberRoles),
    }));

  const tutorialGroupMembers: TutorialGroupMember[] = tutorialGroups.flatMap(
    (tutorialGroup) =>
      faker.random.arrayElements(courseMembers).flatMap((courseMember) => ({
        courseId: courseMember.courseId,
        tutorialGroupId: tutorialGroup.id,
        userId: courseMember.userId,
      })),
  );

  const courseQuestions: Question[] = range(0, 3).map((idx) => {
    const courseMember = faker.random.arrayElement(courseMembers);
    const hasAnswer = faker.random.boolean();
    return {
      id: faker.random.uuid(),
      courseId: courseMember.courseId,
      authorId: courseMember.userId,
      messageText: faker.lorem.paragraphs(1 + faker.random.number(4)),
      messageId: faker.random.uuid(),
      answerId: hasAnswer ? faker.random.uuid() : undefined,
      channelId: faker.random.arrayElement(channelIds),
      qnAPairId: faker.random.uuid(),
      timeStamp: faker.date.recent(14)
    };
  });

  const acceptedAnswers: Answer[] = courseQuestions
    .filter((question) => question.answerId !== undefined)
    .map((question) => {
      const courseMember = faker.random.arrayElement(courseMembers);
      const acceptedByMember = faker.random.arrayElement(courseMembers);
      return {
        id: question.answerId ?? '',
        questionId: question.id,
        channelId: question.channelId,
        courseId: question.courseId,

        authorId: courseMember.userId,
        message: faker.lorem.paragraphs(1 + faker.random.number(4)),
        acceptedById: acceptedByMember.userId,
        messageId: faker.random.uuid(),
      };
    });

  const unacceptedAnswers: Answer[] = range(0, 10).map((idx) => {
    const courseQuestion = faker.random.arrayElement(courseQuestions);
    const randomCourseMember = faker.random.arrayElement(courseMembers);
    return {
      id: faker.random.uuid(),
      questionId: courseQuestion.id,
      courseId: courseQuestion.courseId,
      channelId: courseQuestion.channelId,
      authorId: randomCourseMember.userId,
      message: faker.lorem.paragraphs(1 + faker.random.number(4)),
      acceptedById: undefined,
      messageId: faker.random.uuid(),
    };
  });
  let answers: Answer[] = Array.from(acceptedAnswers).concat(unacceptedAnswers);
  return {
    tutorialGroups,
    courseMembers,
    tutorialGroupMembers,
    courseQuestions,
    answers,
    channels,
  };
}
const icons = [
  'https://fabricweb.azureedge.net/fabric-website/assets/images/avatar/small/ade.jpg',
  'https://fabricweb.azureedge.net/fabric-website/assets/images/avatar/small/elliot.jpg',
  'https://fabricweb.azureedge.net/fabric-website/assets/images/avatar/large/kristy.png',
  'https://fabricweb.azureedge.net/fabric-website/assets/images/avatar/small/nan.jpg',
  'https://fabricweb.azureedge.net/fabric-website/assets/images/avatar/small/matt.jpg',
  'https://fabricweb.azureedge.net/fabric-website/assets/images/avatar/small/steve.jpg',
  'https://fabricweb.azureedge.net/fabric-website/assets/images/avatar/small/nom.jpg',
];

export interface MockDataOptions {
  seed?: number;
  userCount?: number;
  courseCount?: number;
  maxTutorialGroups?: number;
  answerCount?: number;
  kbCount?: number;
}
export function generateFakeData({
  seed,
  userCount,
  courseCount,
  maxTutorialGroups,
  kbCount,
}: MockDataOptions) {
  faker.seed(seed ?? 42);
  const users: User[] = range(0, userCount ?? 100).map((i) => {
    const id = faker.random.uuid();
    return {
      id,
      aadId: id, // for now we're treating this as the same in the client code.
      name: faker.name.findName(),
      upn: faker.internet.email(),
      iconUrl: faker.random.arrayElement(icons),
    }
  });

  const courses: Course[] = range(0, courseCount ?? 10).map((i) => {
    const id = faker.random.uuid();
    return {
      id,
      teamAadObjectId: id,
      teamId: faker.random.uuid(),
      defaultTutorialGroupId: faker.random.uuid(),
      displayName: faker.commerce.productName(),
      hasMultipleTutorialGroups: faker.random.boolean(),
      iconUrl: faker.random.arrayElement(icons),
      knowledgeBaseId: faker.random.boolean() ? faker.random.uuid() : undefined,
    }
  });

  const courseData = courses.map((course) =>
    generateFakeCourseData(course, users, { maxTutorialGroups }),
  );
  const tutorialGroups = courseData.flatMap((d) => d.tutorialGroups);
  const courseMembers = courseData.flatMap((d) => d.courseMembers);
  const tutorialGroupMembers = courseData.flatMap(
    (d) => d.tutorialGroupMembers,
  );
  const questions = courseData.flatMap((d) => d.courseQuestions);
  const answers = courseData.flatMap((d) => d.answers);
  const channels = courseData.flatMap((d) => d.channels);
  const courseEducators = groupBy(courseMembers.filter(m => m.role === 'Educator'), m => m.courseId);
  const attachedKbs: KnowledgeBase[] = courses
    .filter((course) => course.knowledgeBaseId !== undefined)
    .map((course) => ({
      id: course.knowledgeBaseId ?? '',
      name: faker.music.genre(),
      userId: faker.random.arrayElement(courseEducators[course.id]).userId,
    }));
  const additionalKbs: KnowledgeBase[] = range(0, kbCount ?? 5).map((idx) => ({
    id: faker.random.uuid(),
    name: faker.music.genre(),
    userId: faker.random.arrayElement(users).aadId,
  }));
  const knowledgeBases = [...attachedKbs, ...additionalKbs];
  return {
    users,
    tutorialGroups,
    courses,
    courseMembers,
    tutorialGroupMembers,
    questions,
    answers,
    knowledgeBases,
    channels,
  };
}

export interface FakeDataServiceOptions {
  seed: number;
  minLoadTimeMs: number;
  maxLoadTimeMs: number;
}
const DefaultFakeDataServiceOptions: FakeDataServiceOptions = {
  seed: 42,
  minLoadTimeMs: 50,
  maxLoadTimeMs: 400,
};

export class FakeDataService implements QBotService {
  private options: FakeDataServiceOptions;
  private users: Record<string, User>;
  private courses: Course[];
  private tutorialGroups: TutorialGroup[];
  private courseMembers: CourseMember[];
  private tutorialGroupMembers: TutorialGroupMember[];
  private questions: Question[];
  private answers: Answer[];
  private knowledgeBases: KnowledgeBase[];
  private channels: Channel[];

  constructor(options: Partial<FakeDataServiceOptions>) {
    this.options = { ...DefaultFakeDataServiceOptions, ...options };
    const {
      users,
      tutorialGroups,
      courses,
      courseMembers,
      tutorialGroupMembers,
      questions,
      answers,
      knowledgeBases,
      channels,
    } = generateFakeData({
      seed: this.options.seed,
      maxTutorialGroups: 50,
    });
    this.users = toIdMap((u: User) => u.aadId)(users);
    this.courses = courses;
    this.tutorialGroups = tutorialGroups;
    this.courseMembers = courseMembers;
    this.tutorialGroupMembers = tutorialGroupMembers;
    this.questions = questions;
    this.answers = answers;
    this.knowledgeBases = knowledgeBases;
    this.channels = channels;
  }
  async createKnowledgeBase(kb: Omit<KnowledgeBase, 'id'>): Promise<KnowledgeBase> {
    const id = faker.random.uuid();
    const newKb = {
      ...kb,
      id,
    }
    const delayMs = faker.random.number(this.options.maxLoadTimeMs - this.options.minLoadTimeMs) + this.options.minLoadTimeMs;
    await delay(delayMs);
    this.knowledgeBases.push(newKb);
    return newKb;
  }
  async updateKnowledgeBase(kb: KnowledgeBase): Promise<void> {
    const delayMs = faker.random.number(this.options.maxLoadTimeMs - this.options.minLoadTimeMs) + this.options.minLoadTimeMs;
    await delay(delayMs);
    this.knowledgeBases = this.knowledgeBases.filter(k=> k.id !== kb.id).concat([kb]);
  }
  async deleteKnowledgeBase(id: string): Promise<void> {
    const delayMs = faker.random.number(this.options.maxLoadTimeMs - this.options.minLoadTimeMs) + this.options.minLoadTimeMs;
    await delay(delayMs);
    this.knowledgeBases = this.knowledgeBases.filter(k=> k.id !== id);
  }
  async updateTutorialGroup(tutorialGroup: TutorialGroup): Promise<TutorialGroup> {
    const delayMs = faker.random.number(this.options.maxLoadTimeMs - this.options.minLoadTimeMs) + this.options.minLoadTimeMs;
    await delay(delayMs);
    this.tutorialGroups = this.tutorialGroups.filter(tg => tg.id !== tutorialGroup.id).concat([tutorialGroup]);
    return tutorialGroup;
  }
  async deleteTutorialGroup(tutorialGroup: TutorialGroup): Promise<void> {
    const delayMs = faker.random.number(this.options.maxLoadTimeMs - this.options.minLoadTimeMs) + this.options.minLoadTimeMs;
    await delay(delayMs);
    this.tutorialGroups = this.tutorialGroups.filter(tg => tg.id !== tutorialGroup.id);
  }
  async loadUser(userId: string): Promise<User> {
    const delayMs = faker.random.number(this.options.maxLoadTimeMs - this.options.minLoadTimeMs) + this.options.minLoadTimeMs;
    await delay(delayMs);
    console.log('loadUser', {userId});
    const maybeUser = this.users[userId];
    if (!maybeUser) {
      throw new Error('Unknown User');
    }
    return maybeUser;
  }
  async loadAnswer(
    courseId: string,
    channelId: string,
    questionId: string,
    answerId: string,
  ): Promise<Answer> {
    const delayMs = faker.random.number(this.options.maxLoadTimeMs - this.options.minLoadTimeMs) + this.options.minLoadTimeMs;
    await delay(delayMs);
    const answer = this.answers.find(
      (answer) =>
        answer.courseId === courseId &&
        answer.channelId === channelId &&
        answer.questionId === questionId &&
        answer.id == answerId,
    );
    if (!answer) {
      throw new Error('not found');
    }
    return answer;
  }

  async loadChannelsForCourse(courseId: string): Promise<Channel[]> {
    const delayMs = faker.random.number(this.options.maxLoadTimeMs - this.options.minLoadTimeMs) + this.options.minLoadTimeMs;
    await delay(delayMs);
    return this.channels.filter((channel) => channel.courseId === courseId);
  }

  async loadKnowledgeBases(): Promise<KnowledgeBase[]> {
    const delayMs = faker.random.number(this.options.maxLoadTimeMs - this.options.minLoadTimeMs) + this.options.minLoadTimeMs;
    await delay(delayMs);
    return this.knowledgeBases;
  }

  async updateCourse(course: Course): Promise<Course> {
    const delayMs = faker.random.number(this.options.maxLoadTimeMs - this.options.minLoadTimeMs) + this.options.minLoadTimeMs;
    await delay(delayMs);
    this.courses = this.courses
      .filter((c) => c.id != course.id)
      .concat([course]);
    return course;
  }
  async updateQuestion(question: Question): Promise<Question> {
    const delayMs = faker.random.number(this.options.maxLoadTimeMs - this.options.minLoadTimeMs) + this.options.minLoadTimeMs;
    await delay(delayMs);
    this.questions = this.questions
      .filter((q) => q.id != question.id)
      .concat([question]);
    return question;
  }
  async updateAnswer(answer: Answer): Promise<Answer> {
    const delayMs = faker.random.number(this.options.maxLoadTimeMs - this.options.minLoadTimeMs) + this.options.minLoadTimeMs;
    await delay(delayMs);
    this.answers = this.answers
      .filter((a) => a.id != answer.id)
      .concat([answer]);
    return answer;
  }
  async assignTutorialGroup(
    tutorialGroupMember: TutorialGroupMember,
  ): Promise<TutorialGroupMember> {
    const delayMs = faker.random.number(this.options.maxLoadTimeMs - this.options.minLoadTimeMs) + this.options.minLoadTimeMs;
    await delay(delayMs);
    this.tutorialGroupMembers
      .filter(
        (m) =>
          m.courseId !== tutorialGroupMember.courseId ||
          m.userId != tutorialGroupMember.userId,
      )
      .concat([tutorialGroupMember]);
    return tutorialGroupMember;
  }
  async loadTutorialGroupMembersForCourse(
    courseId: string,
  ): Promise<[User[], TutorialGroupMember[]]> {
    const delayMs = faker.random.number(this.options.maxLoadTimeMs - this.options.minLoadTimeMs) + this.options.minLoadTimeMs;
    await delay(delayMs);
    const tutorialGroupMembers = this.tutorialGroupMembers.filter(
      (tg) => tg.courseId === courseId,
    );
    const users = tutorialGroupMembers.map((tg) =>
      this.users[tg.userId],
    ) as User[];
    return [users, tutorialGroupMembers];
  }
  async assignMembership( 
    user: User,
    member: CourseMember,
    tutorialGroupMembers: TutorialGroupMember[]
  ) {
    console.log('assignMembership', {user, member, tutorialGroupMembers});
    const delayMs = faker.random.number(this.options.maxLoadTimeMs - this.options.minLoadTimeMs) + this.options.minLoadTimeMs;
    await delay(delayMs);
    this.courseMembers = this.courseMembers
      .filter(
        (m) => m.courseId !== member.courseId || m.userId != member.userId,
      )
      .concat([member]);
    const tgmIds = new Set(tutorialGroupMembers.map(tgm => tgm.tutorialGroupId));
    this.tutorialGroupMembers = this.tutorialGroupMembers
      .filter(tgm => !(
        (tgm.courseId === member.courseId) && 
        tgmIds.has(tgm.tutorialGroupId) &&
        tgm.userId === user.aadId))
      .concat(tutorialGroupMembers);
  }
  async createTutorialGroup(
    tutorialGroup: TutorialGroup,
  ): Promise<TutorialGroup> {
    const delayMs = faker.random.number(this.options.maxLoadTimeMs - this.options.minLoadTimeMs) + this.options.minLoadTimeMs;
    await delay(delayMs);
    return {
      ...tutorialGroup,
      id: faker.random.uuid(),
    };
  }
  async loadTutorialGroupsForCourse(
    courseId: string,
  ): Promise<TutorialGroup[]> {
    const delayMs = faker.random.number(this.options.maxLoadTimeMs - this.options.minLoadTimeMs) + this.options.minLoadTimeMs;
    await delay(delayMs);
    const tutorialGroups = this.tutorialGroups.filter(
      (tutorialGroup) => tutorialGroup.courseId === courseId,
    );
    return tutorialGroups;
  }
  async loadMembersForTutorialGroup(
    courseId: string,
    tutorialGroupId: string,
  ): Promise<[User[], any[]]> {
    const delayMs = faker.random.number(this.options.maxLoadTimeMs - this.options.minLoadTimeMs) + this.options.minLoadTimeMs;
    await delay(delayMs);
    // const tutorialGroup = this.tutorialGroups.find(tutorialGroup => tutorialGroup.courseId == courseId && tutorialGroup.id === tutorialGroupId);
    const tutorialGroupMembers = this.tutorialGroupMembers.filter(
      (tutorialGroupMember) =>
        tutorialGroupMember.courseId == courseId &&
        tutorialGroupMember.tutorialGroupId === tutorialGroupId,
    );
    const users = tutorialGroupMembers.map((member) =>
      this.users[member.userId],
    ) as User[];
    return [users, tutorialGroupMembers];
  }
  async loadMembersForCourse(courseId: string) {
    const delayMs = faker.random.number(this.options.maxLoadTimeMs - this.options.minLoadTimeMs) + this.options.minLoadTimeMs;
    await delay(delayMs);
    const courseMemberships = this.courseMembers.filter(
      (member) => member.courseId == courseId,
    );
    const userIds = new Set(courseMemberships.map((member) => member.userId));
    const users = values(this.users)
      .filter((user) => userIds.has(user.id))
      .map(user => ({...user, iconUrl: ''})); // wipe the icon url since the API wouldn't be returning it here
    const tutorialGroupMemberships = this.tutorialGroupMembers.filter(tgm => tgm.courseId === courseId);
    console.log('loadMembersForCourse', courseId, { users, courseMemberships, tutorialGroupMemberships, alltgs: this.tutorialGroupMembers });
    return { users, courseMemberships, tutorialGroupMemberships };
  }

  async loadCourses(): Promise<Course[]> {
    const delayMs = faker.random.number(this.options.maxLoadTimeMs - this.options.minLoadTimeMs) + this.options.minLoadTimeMs;
    await delay(delayMs);
    return this.courses;
  }

  async loadQuestions(): Promise<Question[]> {
    const delayMs = faker.random.number(this.options.maxLoadTimeMs - this.options.minLoadTimeMs) + this.options.minLoadTimeMs;
    await delay(delayMs);
    return this.questions;
  }

  async loadAllCourseQuestions(courseId: string) {
    const delayMs = faker.random.number(this.options.maxLoadTimeMs - this.options.minLoadTimeMs) + this.options.minLoadTimeMs;
    await delay(delayMs);
    const questions = this.questions.filter(
      (question) => question.courseId == courseId,
    );
    return questions;
  }
  async loadAnswers(
    courseId: string,
    channelId: string,
    questionId: string,
  ): Promise<Answer[]> {
    const delayMs = faker.random.number(this.options.maxLoadTimeMs - this.options.minLoadTimeMs) + this.options.minLoadTimeMs;
    await delay(delayMs);
    
    const answers = this.answers.filter(
      (answer) =>
        // answer.courseId == courseId &&
        // answer.channelId == channelId &&
        answer.questionId == questionId,
    );
    console.log('loadAnswers', {courseId, channelId, questionId, allAnswers: this.answers, answers})
    return answers;
  }
  async postAnswer(
    courseId: string,
    channelId: string,
    questionId: string,
    answer: Answer,
  ): Promise<Answer> {
    const delayMs = faker.random.number(this.options.maxLoadTimeMs - this.options.minLoadTimeMs) + this.options.minLoadTimeMs;
    await delay(delayMs);
    const merged = {
      ...answer,
      courseId,
      channelId,
      questionId,
    };
    this.answers.push(merged);
    return merged;
  }
}

export type FakeDataServiceType = typeof FakeDataService;
