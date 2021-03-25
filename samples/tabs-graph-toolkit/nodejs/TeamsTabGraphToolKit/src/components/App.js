// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

import React from 'react';
import './App.css';
import * as microsoftTeams from "@microsoft/teams-js";
import { BrowserRouter as Router, Route } from "react-router-dom";
import { Providers, TeamsProvider } from '@microsoft/mgt';
import Privacy from "./Privacy";
import TermsOfUse from "./TermsOfUse";
import Tab from "./Tab";

/**
 * The main app which handles the initialization and routing
 * of the app.
 */
function App() {
  TeamsProvider.microsoftTeamsLib = microsoftTeams;
  Providers.globalProvider = new TeamsProvider ({
      clientId: 'a93a1ba8-d45f-46b0-9288-974ed92d2885',
      authPopupUrl: '/auth.html'
  })
  // Initialize the Microsoft Teams SDK
  microsoftTeams.initialize();

  // Display the app home page hosted in Teams
  return (
    <Router>
      <Route exact path="/privacy" component={Privacy} />
      <Route exact path="/termsofuse" component={TermsOfUse} />
      <Route exact path="/tab" component={Tab} />
    </Router>
  );
}

export default App;
