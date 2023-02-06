import { Answer, Question } from 'models';
import {
  createSlice,
  createEntityAdapter,
  PayloadAction,
} from '@reduxjs/toolkit';

import {
  createCommandHandler,
  iterableCommandHandler,
} from 'createCommandHandler';
import { QuestionService } from 'services/QuestionService';
import { AnswerService } from 'services/AnswerService';
import { answersSlice } from 'stores/answers';
import { globalErrorSlice } from 'stores/globalError';

export const questionAdapter = createEntityAdapter<Question>({
  selectId: (question) => question.id,
  sortComparer: (a, b) => a.timeStamp.valueOf() - b.timeStamp.valueOf(),
});

const initialState = questionAdapter.getInitialState({
  loading: false,
});
export type QuestionState = typeof initialState;

export const questionSlice = createSlice({
  name: 'questions',
  initialState,
  reducers: {
    questionsLoading: (state) => {
      state.loading = true;
    },
    questionsLoaded: (
      state,
      event: PayloadAction<{ questions: Question[] }>,
    ) => {
      state.loading = false;
      questionAdapter.upsertMany(state, event.payload.questions);
    },
  },
});

export function createQuestionHandlers({
  questionService,
  answerService,
}: {
  questionService: QuestionService;
  answerService: AnswerService;
}) {
  async function loadAnswersForQuestions(questions: Question[]) {
    const answeredQuestions = questions.filter(
      (question) => question.answerId !== undefined,
    );
    const answerPromises = answeredQuestions.map((question) =>
      answerService.loadAnswer(
        question.courseId,
        question.channelId,
        question.id,
        question.answerId as string,
      ),
    );
    const answers = await Promise.all(answerPromises);
    const hydratedAnswers = answers.filter(
      (answer) => answer !== undefined,
    ) as Answer[];
    if (answers.length !== hydratedAnswers.length) {
      console.warn('Failed to load some answers', { answers });
    }
    return answersSlice.actions.answersLoaded({ answers: hydratedAnswers });
  }

  async function* loadCourseQuestions(
    command: PayloadAction<{ courseId: string }>,
  ) {
    try {
      const { courseId } = command.payload;
      yield questionSlice.actions.questionsLoading();
      const questions = await questionService.loadAllCourseQuestions(courseId);
      yield loadAnswersForQuestions(questions);
      yield questionSlice.actions.questionsLoaded({ questions });
    } catch (error) {
      yield globalErrorSlice.actions.showError({
        error: error instanceof Error ? error : new Error(`${error}`),
      });
    }
  }
  return createCommandHandler({
    name: 'questionCommands',
    handlers: {
      loadCourseQuestions: iterableCommandHandler(loadCourseQuestions),
      setQuestion: iterableCommandHandler(
        (command: PayloadAction<{ question: Question }>) => [
          questionSlice.actions.questionsLoaded({
            questions: [command.payload.question],
          }),
        ],
      ),
    },
  });
}
export type QuestionHandler = ReturnType<typeof createQuestionHandlers>;
