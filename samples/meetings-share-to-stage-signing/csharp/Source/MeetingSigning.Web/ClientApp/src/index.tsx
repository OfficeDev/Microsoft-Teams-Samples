import React from 'react';
import ReactDOM from 'react-dom';
import { BrowserRouter } from 'react-router-dom';
import { QueryClient, QueryClientProvider } from 'react-query';
import * as microsoftTeams from '@microsoft/teams-js';
import './index.css';
import App from './App';
import reportWebVitals from './reportWebVitals';
import { TeamsProvider } from 'utils/TeamsProvider/TeamsProvider';

declare global {
  // Used to store the token in memory if we can't store it in local storage
  // eslint-disable-next-line no-var
  var anonymousUserAccessToken: string;
}

const queryClient = new QueryClient();

ReactDOM.render(
  <React.StrictMode>
    <TeamsProvider microsoftTeams={microsoftTeams}>
      <BrowserRouter>
        <QueryClientProvider client={queryClient}>
          <App />
        </QueryClientProvider>
      </BrowserRouter>
    </TeamsProvider>
  </React.StrictMode>,
  document.getElementById('root'),
);

// If you want to start measuring performance in your app, pass a function
// to log results (for example: reportWebVitals(console.log))
// or send to an analytics endpoint. Learn more: https://bit.ly/CRA-vitals
reportWebVitals();
