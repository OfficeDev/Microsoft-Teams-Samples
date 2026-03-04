// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

import React, { useEffect } from 'react';
import './App.css';
import * as microsoftTeams from "@microsoft/teams-js";
import { BrowserRouter,Route ,Routes} from 'react-router-dom';

import Privacy from "./Privacy";
import Red from "./Red";
import Gray from "./Gray";
import TabWrapper from "./TabWrapper";
import Home from "./Home";
import TermsOfUse from './Tou';
import TabConfig from './TabConfig';


/**
 * The main app which handles the initialization and routing
 * of the app.
 */
function App() {

  // Initialize the Microsoft Teams SDK
  useEffect(() => {
    (async function () {
        await microsoftTeams.app.initialize();
      })();
    }, []);

  // Display the app home page hosted in Teams
  return (
    <BrowserRouter>
      <Routes>
        <Route path="/" element={<Home />} />
        <Route path="/privacy" element={<Privacy />} />
        <Route path="/gray" element={<Gray />} />
        <Route path="/red" element={<Red />} />
        {/* <Route path="/tab" element={<TabWrapper />}/> */}
        <Route path ="/config" element={<TabWrapper />}/>
        <Route path="/tou" element={<TermsOfUse />} />
      </Routes>
    </BrowserRouter>
  );
}

export default App;
