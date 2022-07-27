import { combineReducers } from '@reduxjs/toolkit';
import { History } from 'history';
import { connectRouter } from 'connected-react-router';
import { tutorialGroupMemberSlice } from 'stores/tutorialGroupMembers';
import { courseSlice } from 'stores/courses';

import { questionSlice } from 'stores/questions';

import { tutorialGroupSlice } from './stores/tutorialGroups';
import { userSlice } from './stores/users';
import { answersSlice } from './stores/answers';
import { channelSlice } from 'stores/channel';
import { courseMemberSlice } from 'stores/courseMembers';
import { globalErrorSlice } from 'stores/globalError';
import { knowledgeBaseSlice } from 'stores/knowledgeBases';

export interface RootReducerParams {
  history: History;
  location?: Location;
}
export function createRootReducer({ history }: RootReducerParams) {
  return combineReducers({
    router: connectRouter(history),
    courses: courseSlice.reducer,
    channels: channelSlice.reducer,
    questions: questionSlice.reducer,
    answers: answersSlice.reducer,
    courseMembers: courseMemberSlice.reducer,
    tutorialGroups: tutorialGroupSlice.reducer,
    tutorialGroupMembers: tutorialGroupMemberSlice.reducer,
    knowledgeBases: knowledgeBaseSlice.reducer,
    users: userSlice.reducer,
    globalError: globalErrorSlice.reducer,
  });
}
