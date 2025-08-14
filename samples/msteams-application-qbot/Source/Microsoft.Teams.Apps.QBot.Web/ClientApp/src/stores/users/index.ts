import { User } from 'models';
import { UserService } from 'services/UserService';
import {
  createSlice,
  createEntityAdapter,
  PayloadAction,
} from '@reduxjs/toolkit';

import {
  asyncCommandHandler,
  createCommandHandler,
} from 'createCommandHandler';

export const usersAdapter = createEntityAdapter<User>({
  selectId: (user) => user.id,
  sortComparer: (a, b) => a.name.localeCompare(b.name),
});

const initialState = usersAdapter.getInitialState({
  loading: false,
});
export type UserState = typeof initialState;

export const userSlice = createSlice({
  name: 'users',
  initialState,
  reducers: {
    usersLoaded(state, action: PayloadAction<{ users: User[] }>) {
      state.loading = false;
      usersAdapter.upsertMany(state, action.payload.users);
    },
    usersLoading(state) {
      state.loading = true;
    },
    userLoaded: usersAdapter.upsertOne,
  },
});

export function createUserHandlers({
  userService,
}: {
  userService: UserService;
}) {
  async function loadUser(command: PayloadAction<{ userId: string }>) {
    const user = await userService.loadUser(command.payload.userId);
    return userSlice.actions.userLoaded(user);
  }
  return createCommandHandler({
    name: 'usersCommands',
    handlers: {
      loadUser: asyncCommandHandler(loadUser),
    },
  });
}
export type UserHandler = ReturnType<typeof createUserHandlers>;
