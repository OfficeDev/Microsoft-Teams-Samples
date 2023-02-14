// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

import React, { useEffect } from "react";
import { Text, LargeTitle, Title2 } from "@fluentui/react-components";
import { app } from "@microsoft/teams-js";

/**
 * The 'PersonalTab' component renders the main tab content
 * of your app.
 */
function Tab() {
  const [teamsContext, setTeamsContext] = React.useState(undefined);
  const [theme, setTheme] = React.useState("default");

  useEffect(() => {
    app.getContext().then((context) => {
      setTeamsContext(context);
      setTheme(context.app.theme);
    });
    app.registerOnThemeChangeHandler((theme) => {
      setTheme(theme);
    });
  }, []);

  return (
    <div>
      <Title2 block>Hello World!</Title2>
      <LargeTitle block>
        Congratulations {teamsContext?.user.userPrincipalName ?? "undefined"}!
      </LargeTitle>
      <Title2 block>This is the tab you made 😀!</Title2>
      <Text as="p" block>
        Theme: {theme}
      </Text>
    </div>
  );
}

export default Tab;
