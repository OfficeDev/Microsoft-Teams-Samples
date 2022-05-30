import { createMatchSelector } from 'connected-react-router';
import { flow, memoize } from 'lodash';
import { createSelector } from 'reselect';
import { QBotState } from 'compositionRoot';
import { selectAnswers } from './answerSelectors';

export const selectPathAnswerId = flow(
  createMatchSelector<QBotState, { answerId?: string }>(
    '/:context/course/:courseId/channel/:channelId/question/:questionId/selectedResponse/:answerId',
  ),
  memoize((match) => match?.params.answerId),
);

export const selectCurrentPathAnswer = createSelector(
  selectAnswers,
  selectPathAnswerId,
  (answers, answerId) => {
    if (answerId === undefined && answers.length === 0) return undefined;
    return answers.find((a) => a.id === answerId);
  },
);

export const selectIsAnswersLoading = (state: QBotState): boolean =>
  state.answers.loading;

export const selectIfAnswersFirstLoading = createSelector(
  selectAnswers,
  selectIsAnswersLoading,
  (answers, isLoading) => answers.length === 0 && isLoading,
);
