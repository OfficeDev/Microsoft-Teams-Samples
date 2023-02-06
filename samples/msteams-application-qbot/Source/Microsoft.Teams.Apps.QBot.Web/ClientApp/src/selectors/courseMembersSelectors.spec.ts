import { QBotState } from 'compositionRoot';
import { selectCourseMemberships } from './courseMembersSelectors';
import fakeState from './fakeState.json';

const baseState = (fakeState as unknown) as QBotState;

describe('selectCourseMemberships', () => {
  it('should select the members array out of the state', () => {
    const members = selectCourseMemberships(baseState);
    expect(members).toHaveLength(13);
    expect(members).toEqual(baseState.courseMembers.members);
  });
});
