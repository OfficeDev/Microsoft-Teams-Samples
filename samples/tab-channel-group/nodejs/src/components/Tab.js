
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
import React from "react";
import * as microsoftTeams from "@microsoft/teams-js";

class ChannelGroupTabConfig extends React.Component {
  constructor(props) {
    super(props);
    this.state = {
      selectedColor: null,
      gray: props.gray || "",
      red: props.red || ""
    };
  }

  componentDidMount() {
    microsoftTeams.app.initialize().then(() => {
      microsoftTeams.pages.config.setValidityState(false); // Disable Save until selection


      microsoftTeams.pages.config.registerOnSaveHandler((saveEvent) => {
        const { selectedColor } = this.state;

        let config = {
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

        {/* Hidden Data Elements */}
        <div style={{ display: "none" }} id="gray">
          {gray}
        </div>
        <div style={{ display: "none" }} id="red">
          {red}
        </div>
      </div>
    );
  }
}

export default ChannelGroupTabConfig;
