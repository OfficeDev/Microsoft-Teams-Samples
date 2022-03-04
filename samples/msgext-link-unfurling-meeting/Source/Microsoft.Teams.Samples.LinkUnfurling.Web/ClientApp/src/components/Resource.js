// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

import React from "react";
import "./App.css";
import * as microsoftTeams from "@microsoft/teams-js";

/**
 * This component is used to display a resource.
 */
class Resource extends React.Component {
  constructor(props) {
    super(props);
  }

  componentDidMount() {
    // Initialize the Microsoft Teams SDK and notify success.
    microsoftTeams.initialize(() =>
      microsoftTeams.appInitialization.notifySuccess()
    );
  }

  render() {
    return (
      <div className="container">
        <h1>{this.props.id}</h1>
        <img
          className="image"
          src="images/image.png"
          alt="Sample dashboard image."
        />
      </div>
    );
  }
}

export default Resource;
