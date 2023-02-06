import React from 'react';
import ReactDOM from 'react-dom';
import { enableMapSet } from 'immer';
import App from './App';
import * as microsoftTeams from '@microsoft/teams-js';
import { TeamsProvider } from 'components/TeamsProvider/TeamsProvider';
import { fakeTeamsSdk } from 'fake/fakeTeams';

// Enable map & set proxy for immer.js
enableMapSet();
const useFakeDataService =
  process.env.REACT_APP_USE_FAKE_DATA === 'true' ||
  window.location.search.indexOf('FAKE_DATA_SERVICE=true') >= 0;

function Loader() {
  return (
    <TeamsProvider
      microsoftTeams={useFakeDataService ? fakeTeamsSdk : microsoftTeams}
    >
      <App />
    </TeamsProvider>
  );
}

ReactDOM.render(<Loader />, document.getElementById('root'));
