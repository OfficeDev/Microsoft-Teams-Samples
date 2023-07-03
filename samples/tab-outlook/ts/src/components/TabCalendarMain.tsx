// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
// Default browser page from where content can directly shared to meeting.

import React from 'react';
import './App.css';

// This component is used to display the required privacy statement which can be found in a link in the about tab.
class TabCalendar extends React.Component {
    render() {
      return (
        <div>
          <h2>Welcome to interacting with the user's calendar, mail, profile from personal tabs app in Outlook.</h2>
        </div>
      );
    }
}

export default TabCalendar;