// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

import React from 'react';
import './App.css';

class Home extends React.Component {
  render() {
    return (
      <div>
        <h1 style={{ color: "#6364a5" }}>Hello, world!</h1>
        <div>
          Welcome to your new application.<br />
        </div>
        <div>
          <button className='appButton' onClick={() => alert("It Works!")}>Click Me!</button>
        </div>
      </div>
    );
  }
}

export default Home;