// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

import React from "react";
import * as microsoftTeams from "@microsoft/teams-js";

/**
 * The 'Config' component is used to display your group tabs
 * user configuration options.  Here you will allow the user to 
 * make their choices and once they are done you will need to validate
 * their choices and communicate that to Teams to enable the save button.
 */
class TabConfig extends React.Component {
  constructor(props) {
    super(props);
    this.state = {
      selectedColor: null,
      gray: props.gray || "Default gray message.",
      red: props.red || "Default red message."
    };
  }

  componentDidMount() {
    microsoftTeams.app.initialize().then(() => {
      microsoftTeams.pages.config.setValidityState(false); // Disable Save until selection

      microsoftTeams.pages.config.registerOnSaveHandler((saveEvent) => {
        const { selectedColor } = this.state;

        const config = {
          websiteUrl: window.location.origin,
          contentUrl: `${window.location.origin}/${selectedColor}`,
          entityId: `${selectedColor}IconTab`,
          suggestedDisplayName: "MyNewTab",
          removeUrl: ""
        };

        microsoftTeams.pages.config.setConfig(config);
        saveEvent.notifySuccess();
      });
    });
  }

  handleColorSelect = (color) => {
    this.setState({ selectedColor: color });
    microsoftTeams.pages.config.setValidityState(true);
  };

  render() {
    const { selectedColor, gray, red } = this.state;

    return (
      <div style={{ padding: 20 }}>

        <div style={{ marginTop: 30 }}>
          {selectedColor === "gray" && (
            <div id="gray" style={{ color: "#666", fontWeight: "bold" }}>
              {gray}
            </div>
          )}

          {selectedColor === "red" && (
            <div id="red" style={{ color: "#900", fontWeight: "bold" }}>
              {red}
            </div>
          )}
        </div>
        <h2 className="disappear" style={{ color: "#6364a5" }}>
          Configure your tab:
        </h2>
        <br />
        <br />

        <div style={{ display: "flex", alignItems: "center", gap: "1rem" }}>
          <button
            className="disappear"
            onClick={() => this.handleColorSelect("gray")}
          >
            Select Gray
          </button>

          <img
            id="icon"
            src={
              selectedColor === "gray"
                ? "/Images/IconGray.png"
                : selectedColor === "red"
                  ? "/Images/IconRed.png"
                  : "/Images/TeamsIcon.png"
            }
            alt="icon"
            style={{ width: "100px" }}
          />

          <button
            className="disappear"
            onClick={() => this.handleColorSelect("red")}
          >
            Select Red
          </button>
        </div>
      </div>
    );
  }
}

export default TabConfig;
