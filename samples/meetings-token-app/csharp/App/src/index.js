import React from 'react';
import ReactDOM from 'react-dom';
import * as microsoftTeams from "@microsoft/teams-js";

import MeetingTokenApp from './Containers/MeetingTokenApp';
import "./style.css";

microsoftTeams.app.initialize().then(() => {
    ReactDOM.render(<MeetingTokenApp />, document.getElementById('root'));
});