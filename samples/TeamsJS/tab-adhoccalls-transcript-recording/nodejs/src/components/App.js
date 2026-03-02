// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

import React from 'react';
import './App.css';
import * as microsoftTeams from "@microsoft/teams-js";
import { BrowserRouter,  Route, Routes } from "react-router-dom";
import RecordingTranscript from "./viewRecordingTranscript";
/**
 * The main app which handles the initialization and routing
 * of the app.
 */
function App() {

  // Initialize the Microsoft Teams SDK
  microsoftTeams.app
  .initialize()
  .then(() => {
    console.log("App.js: initializing client SDK initialized");
    microsoftTeams.app.notifyAppLoaded();
    microsoftTeams.app.notifySuccess();
  })
  .catch((error) => console.error(error));

  // Display the app home page hosted in Teams
  return (
    <BrowserRouter>
    <Routes>
      <Route path="/tab" element={<RecordingTranscript/>} />
    </Routes>
    </BrowserRouter>
  );
}

export default App;
