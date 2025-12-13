// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

import "./Creator.css";

import {
  Avatar
} from "@fluentui/react-components";
import React from "react";

class Creator extends React.Component {
  render() {
    return (
      <div className="creator">
        <Avatar aria-label={this.props.userInfo.displayName} name={this.props.userInfo.displayName} />
        <div className="name">{this.props.userInfo.displayName}</div>
      </div>
    );
  }
}

export default Creator;
