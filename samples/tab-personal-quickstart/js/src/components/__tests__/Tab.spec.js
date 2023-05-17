import React from 'react';
import { render, screen } from '@testing-library/react';
import { act } from 'react-dom/test-utils';
import sinon from 'sinon';
import { app } from '@microsoft/teams-js';
import Tab from '../Tab';

describe('Tab tests', () => {
  var registerOnThemeChangeHandlerStub;
  var themeChangeHandler;

  beforeEach(() => {
    sinon.stub(app, "getContext").returns(
      Promise.resolve({
        user: { userPrincipalName: 'test@test.com' },
        app: { theme: 'default' },
      })
    );

    registerOnThemeChangeHandlerStub = sinon
  .stub(app, 'registerOnThemeChangeHandler')
  .callsFake((handler) => {
    themeChangeHandler = handler;
  });
  });

  afterEach(() => {
    sinon.restore();
  });

  it('Tab contains stubbed contest user principal name', async () => {
    await act(async () => {
      await render(<Tab />);
    });
    expect(screen.getByText(/test@test.com/i)).toBeInTheDocument();
  });

  it('Tab contains Congratulation text', async () => {
    await act(async () => {
      await render(<Tab />);
    });
    expect(screen.getByText(/Congratulations/i)).toBeInTheDocument();
  });

  it('registerOnThemeChangeHandler to be called', async () => {
    await act(async () => {
      await render(<Tab />);
    });

    expect(registerOnThemeChangeHandlerStub.called).toBe(true);
    act(() => {
      themeChangeHandler('dark');
    });
    expect(screen.getByText(/Theme: dark/i)).toBeInTheDocument();
  });
});
