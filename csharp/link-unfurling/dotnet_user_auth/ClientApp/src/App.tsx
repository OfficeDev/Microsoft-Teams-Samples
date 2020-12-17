// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

import React, { useState } from 'react'
import * as microsoftTeams from '@microsoft/teams-js'
import { Provider, themes, Flex, Header, Button, ThemePrepared } from '@fluentui/react'
import * as querystring from 'querystring'
import { isArray } from 'util';

const teamsInitializePromise = new Promise(resolve => microsoftTeams.initialize(() => resolve()));

function getInitialTheme() : ThemePrepared
{
  const params = querystring.parse(window.location.search.substring(1));
  const theme = params["theme"];
  return isArray(theme) 
    ? teamsThemeToFluentTheme(theme[0] || "default") 
    : teamsThemeToFluentTheme(theme);
}

function teamsThemeToFluentTheme(theme: string): ThemePrepared
{
  switch(theme)
  {
    case "dark":
      return themes.teamsDark;
    case "contrast":
      return themes.teamsHighContrast;
    case "default":
    default:
      return themes.teams; 
  }
}

async function registerTeamsThemeHandler(handler: ((theme:ThemePrepared) => void))
{
  await teamsInitializePromise;
  microsoftTeams.registerOnThemeChangeHandler(theme => {
    handler(teamsThemeToFluentTheme(theme))
  });
}

function App() {
  const [theme, setTheme] = useState(getInitialTheme());
  
  registerTeamsThemeHandler(setTheme);

  function onSignout() 
  {
    microsoftTeams.authentication.notifySuccess("signout");
  }

  return (
    <Provider theme={theme} style={{height: "100%", width: "100%", position: "absolute", paddingLeft: "1em"}} >
      <Flex column>
          <Header content="Reddit Link Unfurler" />
          <Button primary content="Sign out" onClick={onSignout}/> 
      </Flex>
    </Provider>
  );
}

export default App;
