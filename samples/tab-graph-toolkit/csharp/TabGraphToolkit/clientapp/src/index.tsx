import React from 'react';
import ReactDOM from 'react-dom';
import App from './components/App';
import { Providers } from '@microsoft/mgt-element';
import { TeamsMsal2Provider } from '@microsoft/mgt-teams-msal2-provider';
import * as MicrosoftTeams from "@microsoft/teams-js";
import { Route, BrowserRouter } from 'react-router-dom';
import { Provider, teamsTheme } from '@fluentui/react-northstar'
import TabAuth from './components/TabAuth';

TeamsMsal2Provider.microsoftTeamsLib = MicrosoftTeams;

Providers.globalProvider = new TeamsMsal2Provider({
   clientId: '813b8a65-e4b2-43d4-afc9-6fd4f88f0ab9',
  authPopupUrl: window.location.origin + '/tabauth',
  scopes: ['calendars.read', 'user.read', 'openid', 'profile', 'people.read', 'user.readbasic.all'],
});

ReactDOM.render(
  <React.StrictMode>
    <Provider theme={teamsTheme}>
       <BrowserRouter >
          <div>
          <Route exact path="/tab" component={App} />
          <Route path="/tabauth" component={TabAuth} />
          </div>     
       </BrowserRouter >
    </Provider >
  </React.StrictMode>,
  document.getElementById('root')
);