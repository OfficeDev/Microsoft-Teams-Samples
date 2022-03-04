// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

import React from "react";
import "./App.css";
import * as microsoftTeams from "@microsoft/teams-js";

/**
 * Message extension settings.
 */
class MESettings extends React.Component {
  constructor(props) {
    super(props);

    // This binding is necessary to make `this` work in the callback
    this.onSignout = this.onSignout.bind(this);
  }

  componentDidMount() {
    // Initialize the Microsoft Teams SDK
    microsoftTeams.initialize();

    // Notify app initialization completion.
    microsoftTeams.appInitialization.notifySuccess();
  }

  onSignout() {
    microsoftTeams.authentication.notifySuccess("signout");
  }

  render() {
    return (
      <div>
        <h1>"Link Unfurling Sample"</h1>
        <button type="button" onClick={this.onSignout}>
          "Sign out"
        </button>
      </div>
    );
  }
}

export default MESettings;
