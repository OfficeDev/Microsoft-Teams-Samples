import React, { useEffect, useState } from "react";
import { Route, Switch } from "react-router";

import { Home } from "./components/Home";
import AuthEnd from "./components/AuthEnd";
import AuthStart from "./components/AuthStart";
import ReviewInMeeting from "./components/ReviewInMeeting";
import Privacy from "./components/Privacy";
import TermsOfUse from "./components/TermsOfUse";
import Resource from "./components/Resource";
import SharedDashboard from "./components/SharedDashboard";
import Config from "./components/Config";
import MESettings from "./components/MESettings";
import { Provider } from "@fluentui/react-northstar";

import * as microsoftTeams from "@microsoft/teams-js";
import {
  teamsDarkTheme,
  teamsHighContrastTheme,
  teamsTheme,
} from "@fluentui/react-northstar";

export const App = () => {
  const [theme, setTheme] = useState(teamsTheme);

  const themeChangeHandler = (theme) => {
    switch (theme) {
      case "dark":
        setTheme(teamsDarkTheme);
        break;
      case "contrast":
        setTheme(teamsHighContrastTheme);
        break;
      case "default":
      default:
        setTheme(teamsTheme);
    }
  };

  useEffect(() => {
    microsoftTeams.initialize(() => {
      microsoftTeams.getContext((context) => {
        themeChangeHandler(context.theme);
      });
      microsoftTeams.registerOnThemeChangeHandler(themeChangeHandler);
    });
  });

  return (
    <Provider theme={theme}>
      <Switch>
        <Route exact path="/" component={Home} />
        <Route exact path="/privacy" component={Privacy} />
        <Route exact path="/termsofuse" component={TermsOfUse} />
        <Route exact path="/auth-start" component={AuthStart} />
        <Route exact path="/auth-end" component={AuthEnd} />
        <Route
          exact
          path="/reviewinmeeting/:id"
          component={(props) => <ReviewInMeeting id={props.match.params.id} />}
        />
        <Route
          exact
          path="/resources/:id"
          component={(props) => <Resource id={props.match.params.id} />}
        />
        <Route exact path="/sharedDashboard" component={SharedDashboard} />
        <Route exact path="/config" component={Config} />
        <Route exact path="/mesettings" component={MESettings} />
      </Switch>
    </Provider>
  );
};
