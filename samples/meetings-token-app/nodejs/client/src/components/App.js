// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

import React from 'react';
//import './App.css';
import * as microsoftTeams from "@microsoft/teams-js";
import { BrowserRouter as Router, Route } from "react-router-dom";

import MeetingTokenApp from '../containers/MeetingTokenApp';
import Privacy from "./Privacy";
import TermsOfUse from "./TermsOfUse";
import Tab from "./Tab";
import TabConfig from "./TabConfig";
import AuthEnd from "./AuthEnd";
import ContentBubble from "./ContentBubble";

/**
 * The main app which handles the initialization and routing
 * of the app.
 */
function App() {

  // Initialize the Microsoft Teams SDK
    microsoftTeams.app.initialize();
  // Display the app home page hosted in Teams
  return (
    <Router>
      <Route exact path="/" component={MeetingTokenApp} />
      <Route exact path="/privacy" component={Privacy} />
      <Route exact path="/termsofuse" component={TermsOfUse} />
      <Route exact path="/tab" component={Tab} />
      <Route exact path="/config" component={TabConfig} />
      <Route exact path="/auth-end" component={AuthEnd} />
      <Route exact path="/bubble" component={ContentBubble} />
    </Router>
  );
}

export default App;
