import { flow } from 'lodash';
import { createSelector } from 'reselect';
import { QBotState } from 'compositionRoot';
import { knowledgeBaseAdapter } from 'stores/knowledgeBases';
import { createMatchSelector } from 'connected-react-router';

const kbSelectors = knowledgeBaseAdapter.getSelectors<QBotState>(
  (state) => state.knowledgeBases,
);
export const selectKnowledgeBases = kbSelectors.selectAll;

// Create a Map<id,course> from the list of all courses
// TODO(nibeauli): evaluate if we can migrate this to record format
export const selectCoursesById = flow(
  selectKnowledgeBases,
  (courses) => new Map(courses.map((c) => [c.id, c])),
);

export const selectIfFirstLoadingKnowledgeBases = createSelector(
  selectKnowledgeBases,
  (state: QBotState) => state.knowledgeBases.loading,
  (kbs, isLoading) => kbs.length === 0 && isLoading,
);

export const selectIfNoKnowledgeBases = createSelector(
  selectKnowledgeBases,
  (state: QBotState) => state.knowledgeBases.loaded,
  (kbs, isLoaded) => kbs.length === 0 && isLoaded,
);

export const selectPathKnowledgeBaseId = flow(
  createMatchSelector<QBotState, { knowledgeBaseId?: string }>(
    '/:context/knowledgeBases/:knowledgeBaseId',
  ),
  (match) => match?.params.knowledgeBaseId,
);

export const selectPathKnowledgeBaseIndex = createSelector(
  selectPathKnowledgeBaseId,
  selectKnowledgeBases,
  (id, kbs) => (id ? kbs.findIndex((kb) => kb.id === id) : -1),
);

export const selectPathKnowledgeBase = createSelector(
  selectPathKnowledgeBaseId,
  (x) => x,
  (id, state) => (id ? kbSelectors.selectById(state, id) : undefined),
);
