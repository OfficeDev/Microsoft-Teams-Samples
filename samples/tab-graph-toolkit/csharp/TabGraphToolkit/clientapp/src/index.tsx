import React from 'react';
import ReactDOM from 'react-dom';
import './index.css';
import App from './components/App';
import reportWebVitals from './reportWebVitals';
import { Providers } from '@microsoft/mgt-element';
import { TeamsMsal2Provider } from '@microsoft/mgt-teams-msal2-provider';
import * as MicrosoftTeams from "@microsoft/teams-js";
import { Route, BrowserRouter } from 'react-router-dom';
import { Provider, teamsTheme } from '@fluentui/react-northstar'
import TabAuth from './components/TabAuth';

TeamsMsal2Provider.microsoftTeamsLib = MicrosoftTeams;

Providers.globalProvider = new TeamsMsal2Provider({
    clientId: 'client_id',
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

// If you want to start measuring performance in your app, pass a function
// to log results (for example: reportWebVitals(console.log))
// or send to an analytics endpoint. Learn more: https://bit.ly/CRA-vitals
reportWebVitals();
