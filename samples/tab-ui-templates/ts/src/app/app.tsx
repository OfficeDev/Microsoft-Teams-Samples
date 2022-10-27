import { useEffect, useState } from "react";
import * as microsoftTeams from "@microsoft/teams-js";
import {
  BrowserRouter as Router,
  Route,
  Switch
} from 'react-router-dom';
import { Provider } from "@fluentui/react-teams";
import { themeNames } from "@fluentui/react-teams";
import { About } from "./about";
import { Privacy } from "./privacy";
import { TermsOfUse } from "./terms_of_use";
import { DashboardTab } from "./static_tabs/dashboard";
import { ListTab } from "./static_tabs/list";
import { BoardsTab } from "./static_tabs/task_boards";
import { WelcomeTab } from "./static_tabs/welcome";

function App() {
  const [appContext, setAppContext] = useState<microsoftTeams.app.Context>();
  const [appAppearance, setAppAppearance] = useState<themeNames>(
    themeNames.Default
  );

  useEffect(() => {
    /**
     * With the context properties in hand, your app has a solid understanding of what's happening around it in Teams.
     * https://docs.microsoft.com/en-us/javascript/api/@microsoft/teams-js/context?view=msteams-client-js-latest&preserve-view=true
     **/
    microsoftTeams.app.getContext().then((context) => {
      setAppContext(context);
      setAppAppearance(initTeamsTheme(context.app.theme));

      /**
       * Tells Microsoft Teams platform that we are done saving our settings. Microsoft Teams waits
       * for the app to call this API before it dismisses the dialog. If the wait times out, you will
       * see an error indicating that the configuration settings could not be saved.
       **/
      microsoftTeams.app.notifySuccess();
    });

    /**
     * Theme change handler
     * https://docs.microsoft.com/en-us/javascript/api/@microsoft/teams-js/microsoftteams?view=msteams-client-js-latest#registerOnThemeChangeHandler__theme__string_____void_
     **/
    microsoftTeams.app.registerOnThemeChangeHandler((theme) => {
      setAppAppearance(initTeamsTheme(theme));
    });
  }, []);

  return (
    <Provider themeName={appAppearance} lang="en-US">
       <Router>
          {/* 
          Default app pages     
        */}
          <Route exact path="/privacy" component={Privacy} />
          <Route exact path="/termsofuse" component={TermsOfUse} />

          {/* 
          Static Tabs 
          To configure it use manifest.json "staticTabs"
        */}
          <Route path="/welcome" component={WelcomeTab} />
          <Route path="/dashboard" component={DashboardTab} />
          <Route path="/list" component={ListTab} />
          <Route path="/board" component={BoardsTab} />
          <Route path="/" component={About} />
        </Router>
    </Provider>
  );
}

export default App;

// Possible values for theme: 'default', 'light', 'dark' and 'contrast'
function initTeamsTheme(theme: string | undefined): themeNames {
  switch (theme) {
    case "dark":
      return themeNames.Dark;
    case "contrast":
      return themeNames.HighContrast;
    default:
      return themeNames.Default;
  }
}
