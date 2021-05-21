import React from 'react';
import ReactDOM from 'react-dom';
import './index.css';
import App from './components/App';
import { Providers } from '@microsoft/mgt-element';
import { Msal2Provider } from '@microsoft/mgt-msal2-provider';
Providers.globalProvider = new Msal2Provider({
  clientId: 'Your_Client_ID',
  scopes: ['calendars.read', 'user.read', 'openid', 'profile', 'people.read', 'user.readbasic.all']
});
ReactDOM.render(
  <React.StrictMode>
    <App />
  </React.StrictMode>,
  document.getElementById('root')
);
