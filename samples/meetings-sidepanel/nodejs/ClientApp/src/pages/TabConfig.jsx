/*!
 * Copyright (c) Microsoft Corporation. All rights reserved.
 * Licensed under the MIT License.
 */

import * as microsoftTeams from "@microsoft/teams-js";
import { useEffect,useState } from "react";
import { getFlexColumnStyles } from "../styles/layouts";
import { mergeClasses, Title2, Subtitle2 } from "@fluentui/react-components";

const TabConfig = () => {

  const [appTheme, setAppTheme] = useState("");

  useEffect(() => {
    microsoftTeams.app.initialize().then(() => {
      microsoftTeams.app.getContext().then((context) => {

        // Applying default theme from app context property
        switch (context.app.theme) {
          case 'dark':
            setAppTheme('theme-dark');
            break;
          case 'default':
            setAppTheme('theme-light');
            break;
          case 'contrast':
            setAppTheme('theme-contrast');
            break;
          default:
            return setAppTheme('theme-light');
        }
      });
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
    <div className={appTheme}>
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
          Welcome to Contoso Media!
        </Title2>
        <Subtitle2 block align="center">
          Press the save button to continue.
        </Subtitle2>
      </div>
    </div>
  );
};

export default TabConfig;
