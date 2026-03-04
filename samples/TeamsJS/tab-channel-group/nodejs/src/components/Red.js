// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

import React from 'react';
import './App.css';

class Red extends React.Component {
  render() {
    return (
      <div>
        <h1>Success!</h1>
        <img id="icon" src="/images/IconRed.png" alt="Red Image" style={{ width: "100px" }} />
        <br />
        <br />
        <p>This is your channel/group tab!</p>
      </div>
    );
  }
}

export default Red;