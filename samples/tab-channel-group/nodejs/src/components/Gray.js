// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

import React from 'react';
import './App.css';

class Gray extends React.Component {
  render() {
    return (
      <div>
        <h1>Success!</h1>
        <img id="icon" src="/images/IconGray.png" alt="Gray Icon" style={{ width: "100px" }} />
        <br />
        <br />
        <p>This is your channel/group tab!</p>
      </div>
    );
  }
}

export default Gray;