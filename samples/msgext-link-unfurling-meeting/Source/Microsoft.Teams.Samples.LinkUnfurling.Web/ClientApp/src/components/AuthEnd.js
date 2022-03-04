// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

import React from "react";
import "./App.css";
import * as microsoftTeams from "@microsoft/teams-js";

/**
 * This component is loaded when the Azure implicit grant flow has completed.
 */
class AuthEnd extends React.Component {
  componentDidMount() {
    microsoftTeams.initialize();

    // The Azure implicit grant flow injects the result into the window.location.hash object. Parse it to find the results.
    let hashParams = this.getHashParameters();

    // If consent has been successfully granted, the Graph access token should be present as a field in the dictionary.
    if (hashParams["access_token"]) {
      // Notifify the showConsentDialogue function in Tab.js that authorization succeeded. The success callback should fire.
      microsoftTeams.authentication.notifySuccess(hashParams["access_token"]);
    } else {
      microsoftTeams.authentication.notifyFailure("Consent failed");
    }
  }

  // Helper function that converts window.location.hash into a dictionary
  getHashParameters() {
    let hashParams = {};
    window.location.hash
      .substr(1)
      .split("&")
      .forEach(function (item) {
        let [key, value] = item.split("=");
        hashParams[key] = decodeURIComponent(value);
      });
    return hashParams;
  }

  render() {
    return (
      <div>
        <h1>Consent flow complete.</h1>
      </div>
    );
  }
}

export default AuthEnd;
