// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

import './App.css';
import * as microsoftTeams from "@microsoft/teams-js";
import { BrowserRouter, Route, Routes } from "react-router-dom";
import Tab from "./Tab";
import Home from './home';

/**
 * The main app which handles the initialization and routing
 * of the app.
 */
function App() {

  // Initialize the Microsoft Teams SDK
  microsoftTeams.app.initialize();

  // Display the app home page hosted in Teams
  return (
    <BrowserRouter>
      <Routes>
        <Route path="/" element={<Home />} />
        <Route path="/tab" element={<Tab />} />
      </Routes>
    </BrowserRouter>
  );
}

export default App;
