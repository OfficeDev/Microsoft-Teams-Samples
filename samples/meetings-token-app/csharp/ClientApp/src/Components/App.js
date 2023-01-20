// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

import React from 'react';
import * as microsoftTeams from "@microsoft/teams-js";
import { BrowserRouter as Router, Route, Routes } from "react-router-dom";

import MeetingTokenApp from '../Containers/MeetingTokenApp';
import TabConfig from "./TabConfig";
import ContentBubble from "./ContentBubble";

// The main app which handles the initialization and routing of the app.
function App() {

  // Initialize the Microsoft Teams SDK
  microsoftTeams.app.initialize();
  
  // Display the app home page hosted in Teams
  return (
    <Router>
      <Routes>
        <Route path="/" element={<MeetingTokenApp />} />
        <Route path="/config" element={<TabConfig />} />
        <Route path="/bubble" element={<ContentBubble />} />
      </Routes>
    </Router>
  );
}

export default App;