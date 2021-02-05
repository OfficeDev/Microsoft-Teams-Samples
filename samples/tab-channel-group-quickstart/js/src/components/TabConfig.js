// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

import React from 'react';
import './App.css';
import * as microsoftTeams from "@microsoft/teams-js";

/**
 * The 'Config' component is used to display your group tabs
 * user configuration options.  Here you will allow the user to 
 * make their choices and once they are done you will need to validate
 * their choices and communicate that to Teams to enable the save button.
 */
class TabConfig extends React.Component {

    render() {
      /**
       * When the user clicks "Save", save the url for your configured tab.
       * This allows for the addition of query string parameters based on
       * the settings selected by the user.
       */
      microsoftTeams.settings.registerOnSaveHandler((saveEvent) => {

        const baseUrl = `https://${window.location.hostname}:${window.location.port}`;
        microsoftTeams.settings.setSettings({
          "suggestedDisplayName": "My Tab",
          "entityId": "Test",
          "contentUrl": baseUrl + "/tab",
          "websiteUrl": baseUrl + "/tab"
        });
        saveEvent.notifySuccess();
       });
  
      /**
       * After verifying that the settings for your tab are correctly
       * filled in by the user you need to set the state of the dialog
       * to be valid.  This will enable the save button in the configuration
       * dialog.
       */
      microsoftTeams.settings.setValidityState(true);
  
      return (
        <div>
          <h1>Tab Configuration</h1>
          <div>
            This is where you will add your tab configuration options the user
            can choose when the tab is added to your team/group chat.            
          </div>
        </div>
      );
    }
  }

  export default TabConfig;