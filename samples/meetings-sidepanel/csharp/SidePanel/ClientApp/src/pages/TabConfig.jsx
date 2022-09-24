/*!
 * Copyright (c) Microsoft Corporation. All rights reserved.
 * Licensed under the MIT License.
 */

import * as microsoftTeams from "@microsoft/teams-js";
import { useEffect } from "react";
import { getFlexColumnStyles } from "../styles/layouts";
import { mergeClasses, Title2, Subtitle2 } from "@fluentui/react-components";

const TabConfig = () => {
  useEffect(() => {
    microsoftTeams.app.initialize().then(() => {
      microsoftTeams.pages.config.registerOnSaveHandler(function (saveEvent) {
        microsoftTeams.pages.config.setConfig({
          suggestedDisplayName: "Live Coding",
          contentUrl: `${window.location.origin}/sidepanel`,
        });
        saveEvent.notifySuccess();
      });

      microsoftTeams.pages.config.setValidityState(true);
    });
  }, []);

  const flexColumnStyles = getFlexColumnStyles();
  return (
    <div
      className={mergeClasses(
        flexColumnStyles.root,
        flexColumnStyles.hAlignCenter,
        flexColumnStyles.vAlignCenter,
        flexColumnStyles.fill,
        flexColumnStyles.smallGap
      )}
    >
      <Title2 block align="center">
        Welcome to Meeting Side Panel App !
      </Title2>
      <Subtitle2 block align="center">
        Press the save button to continue.
      </Subtitle2>
    </div>
  );
};

export default TabConfig;