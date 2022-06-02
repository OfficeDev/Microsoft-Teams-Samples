// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

import React from "react";
import "./App.css";
import * as microsoftTeams from "@microsoft/teams-js";

/**
 * Displays shared dashboard
 */
class SharedDashboard extends React.Component {
  componentDidMount() {
    // Initialize the Microsoft Teams SDK and notify success.
    microsoftTeams.initialize(() =>
      microsoftTeams.appInitialization.notifySuccess()
    );
  }

  render() {
    return (
      <div className="container">
        <h1>Shared Dashboard</h1>
        <img
          className="image"
          src="images/power-bi-dashboard.png"
          alt="Sample dashboard image."
        />
      </div>
    );
  }
}

export default SharedDashboard;
