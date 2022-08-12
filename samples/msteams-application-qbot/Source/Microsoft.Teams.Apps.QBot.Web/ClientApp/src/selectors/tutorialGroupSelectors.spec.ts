import { QBotState } from 'compositionRoot';
import { selectTutorialGroups } from './tutorialGroupSelectors';
import fakeState from './fakeState.json';

const baseState = (fakeState as unknown) as QBotState;
describe('selectTutorialGroups', () => {
  it('should get the list of all the tutorial groups', () => {
    const tutorialGroups = selectTutorialGroups(baseState);
    expect(tutorialGroups).toEqual(baseState.tutorialGroups.tutorialGroups);
  });
});
