// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

import React, { useEffect } from "react";
import "./App.css";
import { app } from "@microsoft/teams-js";

/**
 * The 'PersonalTab' component renders the main tab content
 * of your app.
 */
function Tab() {
  const [teamsContext, setTeamsContext] = React.useState(undefined);

  useEffect(() => {
    app.getContext().then((context) => {
      setTeamsContext(context);
    });
  }, []);

  return (
    <div>
      <h3>Hello World!</h3>
      <h1>Congratulations {teamsContext?.user.userPrincipalName ?? "undefined"}!</h1>
      <h3>This is the tab you made :-)</h3>
    </div>
  );
}

export default Tab;
