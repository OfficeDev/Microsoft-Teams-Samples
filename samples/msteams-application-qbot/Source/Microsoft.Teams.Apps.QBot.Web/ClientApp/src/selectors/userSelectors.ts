import { flow, memoize } from 'lodash';
import { createSelector } from 'reselect';
import { QBotState } from 'compositionRoot';
import { usersAdapter } from 'stores/users';

const usersSelectors = usersAdapter.getSelectors<QBotState>(
  (state) => state.users,
);

export const selectUsers = usersSelectors.selectAll;
export const selectUsersById = flow(
  selectUsers,
  memoize((users) => new Map(users.map((u) => [u.id, u]))),
);
export const selectUsersByAadId = flow(
  selectUsers,
  memoize((users) => new Map(users.map((u) => [u.aadId, u]))),
);

export const selectIfUsersFirstLoading = createSelector(
  selectUsers,
  (state: QBotState) => state.users.loading,
  (users, isLoading) => users.length === 0 && isLoading,
);
