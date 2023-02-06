import { flow } from 'lodash';
import { createSelector } from 'reselect';
import { QBotState } from 'compositionRoot';
import { answersAdapter } from 'stores/answers';

const answersSelectors = answersAdapter.getSelectors<QBotState>(
  (state) => state.answers,
);

export const selectAnswers = answersSelectors.selectAll;

// Create a Map<id,answer> from the list of all answers
export const selectAnswersById = flow(
  selectAnswers,
  (answers) => new Map(answers.map((a) => [a.id, a])),
);

export const selectIfFirstLoadingAnswers = createSelector(
  selectAnswers,
  (state: QBotState) => state.answers.loading,
  (answers, isLoading) => answers.length === 0 && isLoading,
);

export const selectIsAnswerPosting = (state: QBotState): boolean =>
  state.answers.posting;
