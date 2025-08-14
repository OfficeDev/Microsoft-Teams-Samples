// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

import React from 'react';
import './App.css';
import * as microsoftTeams from "@microsoft/teams-js";

/**
 * The 'Config' component is used to display your group tabs
 * user configuration options. Here you will allow the user to 
 * make their choices and once they are done you will need to validate
 * their choices and communicate that to Teams to enable the save button.
 */
class TabConfig extends React.Component {

  componentDidMount() {
    microsoftTeams.app.initialize().then(() => {
      /**
       * When the user clicks "Save", save the url for your configured tab.
       * This allows for the addition of query string parameters based on
       * the settings selected by the user.
       */
      microsoftTeams.pages.config.registerOnSaveHandler((saveEvent) => {
        microsoftTeams.pages.config.setConfig({
          "suggestedDisplayName": "Token App",
          "entityId": "TokenAppTab",
          "contentUrl": window.location.origin + "/",
          "websiteUrl": window.location.origin + "/"
        });
        saveEvent.notifySuccess();
      });

      /**
       * After verifying that the settings for your tab are correctly
       * filled in by the user you need to set the state of the dialog
       * to be valid. This will enable the save button in the configuration
       * dialog.
       */
      microsoftTeams.pages.config.setValidityState(true);
    });
  }

  render() {

    return (
      <div>
        <h1>Tab Configuration</h1>
        <div>
          This is where you add your tab configuration options the user
          can choose when the tab is added to your team/group chat.
        </div>
      </div>
    );
  }
}

export default TabConfig;