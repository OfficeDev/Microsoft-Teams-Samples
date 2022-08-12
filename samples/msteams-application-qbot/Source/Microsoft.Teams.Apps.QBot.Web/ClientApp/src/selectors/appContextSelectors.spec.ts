/* eslint-disable max-lines-per-function */
import { QBotState } from 'compositionRoot';
import { createLocation } from 'history';
import { selectAppContext } from './appContextSelectors';

describe('selectAppContext', () => {
  it('should populate the context variable with personal when the path starts with /personal/', () => {
    const state = {
      router: {
        location: createLocation('/personal/rest/of/path'),
        action: 'PUSH',
      },
    } as QBotState;
    const context = selectAppContext(state);
    expect(context).toEqual('personal');
  });

  it('should populate the context variable with team when the path starts with /team/', () => {
    const state = {
      router: {
        location: createLocation('/team/rest/of/path'),
        action: 'PUSH',
      },
    } as QBotState;
    const context = selectAppContext(state);
    expect(context).toEqual('team');
  });

  it('should populate the context variable with taskModule when the path starts with /taskModule/', () => {
    const state = {
      router: {
        location: createLocation('/taskModule/rest/of/path'),
        action: 'PUSH',
      },
    } as QBotState;
    const context = selectAppContext(state);
    expect(context).toEqual('taskModule');
  });

  it('should return undefined with the path does not begin with a known context (personal|team|taskModule)', () => {
    const state = {
      router: {
        location: createLocation('/unknown/prefix/'),
        action: 'PUSH',
      },
    } as QBotState;
    const context = selectAppContext(state);
    expect(context).toBeUndefined();
  });
});
