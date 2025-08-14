import { createSelector } from 'reselect';
import { QBotState } from 'compositionRoot';
import { tutorialGroupAdapter } from 'stores/tutorialGroups';
const tutorialGroupSelectors = tutorialGroupAdapter.getSelectors(
  (state: QBotState) => state.tutorialGroups,
);
export const selectTutorialGroups = tutorialGroupSelectors.selectAll;

export const selectTutorialGroupsById = createSelector(
  selectTutorialGroups,
  (tutorialGroups) => new Map(tutorialGroups.map((tg) => [tg.id, tg])),
);

export const selectIfTutorialGroupsFirstLoading = createSelector(
  selectTutorialGroups,
  (state: QBotState) => state.tutorialGroups.loading,
  (tutorialGroups, isLoading) => tutorialGroups.length === 0 && isLoading,
);
