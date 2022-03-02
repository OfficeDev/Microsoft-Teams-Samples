// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

import React from "react";
import "./App.css";
import * as microsoftTeams from "@microsoft/teams-js";
import { Header } from "@fluentui/react-northstar";

/**
 * This component is used to display tab configuration.
 */
class Config extends React.Component {
  componentDidMount() {
    // Initialize the Microsoft Teams SDK
    microsoftTeams.initialize();

    // Notify app initialization completion.
    microsoftTeams.appInitialization.notifySuccess();

    // No configuration supported, so set validity state to true.
    microsoftTeams.settings.setValidityState(true);

    // Save settings..
    microsoftTeams.settings.registerOnSaveHandler((saveEvent) => {
      microsoftTeams.settings.setSettings({
        websiteUrl: `${process.env.REACT_APP_BASE_URL}`,
        contentUrl: `${process.env.REACT_APP_BASE_URL}/SharedDashboard`,
        entityId: "",
        suggestedDisplayName: "Shared dashboard",
      });
      saveEvent.notifySuccess();
    });
  }

  render() {
    return (
      <div className="container">
        <Header as="h1">Configuration</Header>
      </div>
    );
  }
}

export default Config;
