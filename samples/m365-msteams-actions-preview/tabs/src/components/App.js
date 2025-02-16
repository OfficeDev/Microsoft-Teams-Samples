// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import "./App.css";

import * as microsoftTeams from "@microsoft/teams-js";

import { Route, HashRouter as Router, Routes } from "react-router-dom";

import DialogPage from "./DialogPage";
import Privacy from "./about/Privacy";
import React from "react";
import Tab from "./Tab";
import TabConfig from "./TabConfig";
import TermsOfUse from "./about/TermsOfUse";

/**
 * The main app which handles the initialization and routing
 * of the app.
 */
function App() {
  // Check for the Microsoft Teams SDK object.
  if (microsoftTeams) {
    return (
      <Router>
        <Routes>
          <Route path="/privacy" element={<Privacy />} />
          <Route path="/termsofuse" element={<TermsOfUse />} />
          <Route path="/tab" element={<Tab />} />
          <Route path="/config" element={<TabConfig />} />
          <Route path="/dialogPage" element={<DialogPage />} />
        </Routes>
      </Router>
    );
  } else {
    return <h3>Microsoft Teams SDK not found.</h3>;
  }
}

export default App;
