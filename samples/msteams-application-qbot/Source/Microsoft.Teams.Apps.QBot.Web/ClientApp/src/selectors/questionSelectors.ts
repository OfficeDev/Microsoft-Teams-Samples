import { flow } from 'lodash';
import { FullQuestion } from 'models';
import { createSelector } from 'reselect';
import { QBotState } from 'compositionRoot';
import { selectAnswersById } from './answerSelectors';
import { selectChannelsById } from './channelsSelectors';
import { selectUsersById } from './userSelectors';
import { questionAdapter } from 'stores/questions';
import { selectCurrentPathCourseId } from './currentCourseSelectors';
import { isNotNullOrUndefined } from 'util/isNotNullOrUndefined';

const questionsSelectors = questionAdapter.getSelectors<QBotState>(
  (state) => state.questions,
);

export const selectQuestions = questionsSelectors.selectAll;

export const selectIsQuestionsLoading = (state: QBotState): boolean =>
  state.questions.loading;

// Create a Map<id,question> from the list of all questions
export const selectQuestionsById = flow(
  selectQuestions,
  (questions) => new Map(questions.map((q) => [q.id, q])),
);

export const selectIfQuestionsFirstLoading = createSelector(
  selectQuestions,
  (state: QBotState) => state.questions.loading,
  (questions, isLoading) => questions.length === 0 && isLoading,
);

export const selectFullQuestions = createSelector(
  selectUsersById,
  selectQuestions,
  selectAnswersById,
  selectChannelsById,
  (usersById, questions, answersById, channelsById) =>
    questions
      .map<FullQuestion | null>((q) => {
        const questionChannel = channelsById.get(q.channelId) || { name: '' };
        const user = usersById.get(q.authorId);
        if (!user) {
          console.warn(
            'filtering out question since associated user is not loaded',
            { question: q },
          );
          return null;
        }

        return {
          ...q,
          user,
          answer: q.answerId ? answersById.get(q.answerId) : undefined,
          channelName: questionChannel.name,
        };
      })
      .filter(isNotNullOrUndefined),
);

export const selectCurrentCourseQuestions = createSelector(
  selectCurrentPathCourseId,
  selectFullQuestions,
  (courseId, questions) => questions.filter((q) => q.courseId === courseId),
);
