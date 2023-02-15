import React from 'react';
import { render, screen } from '@testing-library/react';
import { act } from 'react-dom/test-utils';
import sinon from 'sinon';
import { app } from '@microsoft/teams-js';
import Tab from "../Tab";

sinon.stub(app, 'getContext').returns(
  Promise.resolve({
    user: { userPrincipalName: 'test@test.com' },
    app: { theme: 'default' },
  })
);
const registerOnThemeChangeHandlerStub = sinon
  .stub(app, 'registerOnThemeChangeHandler');

describe('Tab tests', () => {
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
  });
});
