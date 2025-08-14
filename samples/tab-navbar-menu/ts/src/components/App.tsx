// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

import React from 'react';
import './App.css';
import * as microsoftTeams from "@microsoft/teams-js";
import { BrowserRouter as Router, Route } from "react-router-dom";
import Tab from "./Tab";
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
      <Route exact path="/tab" component={Tab} />
    </Router>
  );
}

export default App;
