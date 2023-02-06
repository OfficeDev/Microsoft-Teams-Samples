import { TutorialGroup } from 'models';
import {
  createSlice,
  createEntityAdapter,
  PayloadAction,
} from '@reduxjs/toolkit';

import {
  asyncCommandHandler,
  createCommandHandler,
  iterableCommandHandler,
} from 'createCommandHandler';
import { TutorialGroupService } from 'services/TutorialGroupsService';

export const tutorialGroupAdapter = createEntityAdapter<TutorialGroup>({
  selectId: (tutorialGroup) => `${tutorialGroup.courseId}-${tutorialGroup.id}`,
  sortComparer: (a, b) => a.displayName.localeCompare(b.displayName),
});

const initialState = tutorialGroupAdapter.getInitialState({
  loading: false,
});

export type TutorialGroupState = typeof initialState;

export const tutorialGroupSlice = createSlice({
  name: 'tutorialGroups',
  initialState,
  reducers: {
    setTutorialGroup: tutorialGroupAdapter.upsertOne,
    deleteTutorialGroup: tutorialGroupAdapter.removeOne,
    tutorialGroupsLoading: (state) => {
      state.loading = true;
    },
    tutorialGroupsLoaded(
      state,
      action: PayloadAction<{ tutorialGroups: TutorialGroup[] }>,
    ) {
      state.loading = false;
      tutorialGroupAdapter.upsertMany(state, action.payload.tutorialGroups);
    },
  },
});

export function createTutorialGroupHandlers({
  tutorialGroupService,
}: {
  tutorialGroupService: TutorialGroupService;
}) {
  async function* loadTutorialGroups(
    command: PayloadAction<{ courseId: string }>,
  ) {
    yield tutorialGroupSlice.actions.tutorialGroupsLoading();
    try {
      const tutorialGroups = await tutorialGroupService.loadTutorialGroupsForCourse(
        command.payload.courseId,
      );
      yield tutorialGroupSlice.actions.tutorialGroupsLoaded({ tutorialGroups });
    } catch (ex) {
      console.warn('Failed to load tutorialGroups', { ex });
      yield tutorialGroupSlice.actions.tutorialGroupsLoaded({
        tutorialGroups: [],
      });
    }
  }
  async function createTutorialGroup({
    payload: { tutorialGroup },
  }: PayloadAction<{ tutorialGroup: TutorialGroup }>) {
    const responseTutorialGroup = await tutorialGroupService.createTutorialGroup(
      tutorialGroup,
    );
    return tutorialGroupSlice.actions.setTutorialGroup(responseTutorialGroup);
  }
  async function editCommandHandler({
    payload: { tutorialGroup },
  }: PayloadAction<{ tutorialGroup: TutorialGroup }>) {
    const responseTutorialGroup = await tutorialGroupService.updateTutorialGroup(
      tutorialGroup,
    );
    return tutorialGroupSlice.actions.setTutorialGroup(responseTutorialGroup);
  }
  async function deleteCommandHandler({
    payload: { tutorialGroup },
  }: PayloadAction<{ tutorialGroup: TutorialGroup }>) {
    await tutorialGroupService.deleteTutorialGroup(tutorialGroup);
    return tutorialGroupSlice.actions.deleteTutorialGroup(tutorialGroup.id);
  }
  return createCommandHandler({
    name: 'tutorialGroupCommands',
    handlers: {
      loadTutorialGroups: iterableCommandHandler(loadTutorialGroups),
      createTutorialGroup: asyncCommandHandler(createTutorialGroup),
      editTutorialGroup: asyncCommandHandler(editCommandHandler),
      deleteCommandHandler: asyncCommandHandler(deleteCommandHandler),
    },
  });
}
export type TutorialGroupHandlers = ReturnType<
  typeof createTutorialGroupHandlers
>;
