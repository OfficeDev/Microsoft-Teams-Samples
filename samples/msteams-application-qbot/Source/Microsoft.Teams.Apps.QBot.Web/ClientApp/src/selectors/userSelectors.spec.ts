import { QBotState } from 'compositionRoot';
import {
  selectUsers,
  selectUsersByAadId,
  selectUsersById,
} from './userSelectors';
import fakeState from './fakeState.json';

const baseState = (fakeState as unknown) as QBotState;

describe('selectUsers', () => {
  it('should get the users out of the state', () => {
    const users = selectUsers(baseState);
    expect(users).toEqual(baseState.users.users);
  });
});

describe('selectUsersByAadId', () => {
  it('should get the users as a map by their aadId', () => {
    const user = baseState.users.users[0];
    const usersByAadIdMap = selectUsersByAadId(baseState);
    expect(usersByAadIdMap.get(user.aadId)).toEqual(user);
  });
});

describe('selectUsersById', () => {
  it('should get the users as a map by their id', () => {
    const user = baseState.users.users[0];
    const usersByAadIdMap = selectUsersById(baseState);
    expect(usersByAadIdMap.get(user.id)).toEqual(user);
  });
});
