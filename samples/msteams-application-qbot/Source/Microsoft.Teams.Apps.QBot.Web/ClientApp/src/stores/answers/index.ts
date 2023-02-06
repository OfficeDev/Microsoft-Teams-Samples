import { Answer } from 'models';
import {
  createSlice,
  createEntityAdapter,
  PayloadAction,
} from '@reduxjs/toolkit';

import {
  createCommandHandler,
  iterableCommandHandler,
} from 'createCommandHandler';
import { AnswerService } from 'services/AnswerService';
import { UserService } from 'services/UserService';
import { userSlice } from 'stores/users';
import * as microsoftTeams from '@microsoft/teams-js';
import { globalErrorSlice } from 'stores/globalError';

export const answersAdapter = createEntityAdapter<Answer>({
  selectId: (answer) => answer.id,
  // There isn't really a logical 'ordering' to answer entities, just compare
  // the ids.
  sortComparer: (a, b) => a.id.localeCompare(b.id),
});

const initialState = answersAdapter.getInitialState<{
  loading: boolean;
  posting: boolean;
}>({
  loading: false,
  posting: false,
});

export type AnswerState = typeof initialState;

export const answersSlice = createSlice({
  name: 'answers',
  initialState,
  reducers: {
    answersLoaded(state, event: PayloadAction<{ answers: Answer[] }>) {
      state.loading = false;
      state.posting = false;
      answersAdapter.upsertMany(state, event.payload.answers);
    },
    answersLoading(state) {
      state.loading = true;
    },
    answerPosting(state) {
      state.posting = true;
    },
  },
});

export function createAnswersHandlers({
  answerService,
  userService,
}: {
  answerService: AnswerService;
  userService: UserService;
}) {
  async function* loadAnswers(
    command: PayloadAction<{
      courseId: string;
      channelId: string;
      questionId: string;
    }>,
  ) {
    const { courseId, channelId, questionId } = command.payload;
    yield answersSlice.actions.answersLoading();
    try {
      const answers = await answerService.loadAnswers(
        courseId,
        channelId,
        questionId,
      );
      const users = await Promise.all(
        answers.map((answer) => userService.loadUser(answer.authorId)),
      );
      yield answersSlice.actions.answersLoaded({ answers });
      yield userSlice.actions.usersLoaded({ users });
    } catch (error) {
      yield globalErrorSlice.actions.showError({
        error: error instanceof Error ? error : new Error(`${error}`),
      });
    }
  }
  async function* loadAnswer(
    command: PayloadAction<{
      courseId: string;
      channelId: string;
      questionId: string;
      answerId: string;
    }>,
  ) {
    const { courseId, channelId, questionId, answerId } = command.payload;
    yield answersSlice.actions.answersLoading();
    const answer = await answerService.loadAnswer(
      courseId,
      channelId,
      questionId,
      answerId,
    );
    const users = await userService.loadUser(answer.authorId);
    yield answersSlice.actions.answersLoaded({ answers: [answer] });
    yield userSlice.actions.usersLoaded({ users: [users] });
  }

  async function* handlePostAnswerCommand(
    command: PayloadAction<{
      courseId: string;
      channelId: string;
      questionId: string;
      answer: Answer;
    }>,
  ) {
    try {
      yield answersSlice.actions.answerPosting();
      const { courseId, channelId, questionId, answer } = command.payload;

      await answerService.postAnswer(courseId, channelId, questionId, answer);
      // TODO: this should be injected
      microsoftTeams.tasks.submitTask(undefined);
    } catch (error) {
      yield globalErrorSlice.actions.showError({
        error: error instanceof Error ? error : new Error(`${error}`),
      });
    }
  }
  return createCommandHandler({
    name: 'answerCommands',
    handlers: {
      loadAnswers: iterableCommandHandler(loadAnswers),
      loadAnswer: iterableCommandHandler(loadAnswer),
      setAnswer: iterableCommandHandler(
        (command: PayloadAction<{ answer: Answer }>) => [
          answersSlice.actions.answersLoaded({
            answers: [command.payload.answer],
          }),
        ],
      ),
      postAnswer: iterableCommandHandler(handlePostAnswerCommand),
    },
  });
}
export type AnswerHandler = ReturnType<typeof createAnswersHandlers>;
