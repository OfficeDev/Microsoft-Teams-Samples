/*!
 * Copyright (c) Microsoft Corporation. All rights reserved.
 * Licensed under the MIT License.
 */

import { FluentProvider, teamsDarkTheme } from "@fluentui/react-components";
import * as microsoftTeams from "@microsoft/teams-js";
import { useEffect, useState } from "react";
import { BrowserRouter as Router, Routes, Route } from "react-router-dom";
import SidePanel from "./pages/SidePanel";
import TabConfig from "./pages/TabConfig";
import { inTeams } from "./utils/inTeams";

export default function App() {
  const [initialized, setInitialized] = useState(false);

  useEffect(() => {
    if (!initialized) {
      if (inTeams()) {
        console.log("App.js: initializing client SDK");
        microsoftTeams.app
          .initialize()
          .then(() => {
            console.log("App.js: initializing client SDK initialized");
            microsoftTeams.app.notifyAppLoaded();
            microsoftTeams.app.notifySuccess();
            setInitialized(true);
          })
          .catch((error) => console.error(error));
      } else {
        setInitialized(true);
      }
    }
  }, []);

  if (!initialized) {
    return <div />;
  }

  return (
    <FluentProvider
      theme={teamsDarkTheme}
      style={{
        minHeight: "0px",
        position: "absolute",
        left: "0",
        right: "0",
        top: "0",
        bottom: "0",
        overflow: "hidden",
        backgroundColor: inTeams() ? "transparent" : "#202020",
      }}
    >
      <Router window={window} basename="/">
        <Routes>
          <Route exact path={"/sidepanel"} element={<SidePanel />} />
          <Route exact path={"/config"} element={<TabConfig />} />
        </Routes>
      </Router>
    </FluentProvider>
  );
}
