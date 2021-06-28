import React from 'react';
import ReactDOM from 'react-dom';
import './index.css';
import App from './App';
import * as microsoftTeams from "@microsoft/teams-js";
import TeamsContexProvider from './provider/TeamsContextProvider'
import reportWebVitals from './reportWebVitals';

microsoftTeams.initialize();

ReactDOM.render(
  <React.StrictMode>
    <TeamsContexProvider>
      <App/>
    </TeamsContexProvider>
  </React.StrictMode>,
  document.getElementById('root')
);

reportWebVitals();
