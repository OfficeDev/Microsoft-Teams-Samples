/**
 * Copyright (c) Microsoft Corporation. All rights reserved.
 * Licensed under the MIT License.
 */

import ReactDOM from "react-dom";
import App from "./app/app";
import * as microsoftTeams from "@microsoft/teams-js";
import reportWebVitals from "./report-web-vitals";

// Initialize the Microsoft Teams SDK
// Note: The initialization method below works correctly in the M365 environment.
// If we use `microsoftTeams.app.initialize().then(async () => {})`, M365 does not load and gives an error: "SDK is not initialized."  
microsoftTeams.app.initialize();

  ReactDOM.render(
      <App />,
    document.getElementById("root")
  );

// If you want to start measuring performance in your app, pass a function
// to log results (for example: reportWebVitals(console.log))
// or send to an analytics endpoint. Learn more: https://bit.ly/CRA-vitals
reportWebVitals();