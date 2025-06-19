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
        try {
            microsoftTeams.initialize(() => {
                let hashParams = this.getHashParameters();

                if (hashParams["access_token"]) {
                    microsoftTeams.authentication.notifySuccess(hashParams["access_token"]);
                } else {
                    microsoftTeams.authentication.notifyFailure("Consent failed");
                }
            });
        } catch (err) {
            console.error("Teams SDK initialization failed:", err);
            microsoftTeams.authentication.notifyFailure("Teams SDK not initialized");
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
