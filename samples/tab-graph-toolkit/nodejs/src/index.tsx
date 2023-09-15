import React from 'react';
import ReactDOM from 'react-dom';
import App from './components/App';
import * as MicrosoftTeams from "@microsoft/teams-js";
import { Providers } from '@microsoft/mgt';
import { TeamsMsal2Provider } from '@microsoft/mgt-teams-msal2-provider';
import './index.css';

TeamsMsal2Provider.microsoftTeamsLib = MicrosoftTeams;

Providers.globalProvider = new TeamsMsal2Provider({
  clientId: '2c865108-9c5e-456c-94c3-bb792a77576b',
  authPopupUrl: window.location.origin + '/tabauth',
  scopes: ['calendars.read', 'user.read', 'openid', 'profile', 'people.read', 'user.readbasic.all'],
});

ReactDOM.render(
  <React.StrictMode>
    <App />
  </React.StrictMode>,
  document.getElementById('root')
);
