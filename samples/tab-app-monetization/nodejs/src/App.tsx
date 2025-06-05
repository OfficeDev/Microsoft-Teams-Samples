import React, { Suspense } from 'react';
import './App.css';
import { BrowserRouter, Routes, Route   } from 'react-router-dom';
import { app } from "@microsoft/teams-js";
import { TeamsThemeContext, getContext, ThemeStyle } from 'msteams-ui-components-react';
import { FluentProvider, teamsDarkTheme, teamsHighContrastTheme, teamsLightTheme } from '@fluentui/react-components';
import Tab from './components/tab';

export interface IAppState {
  theme: string;
  themeStyle: number;
}

class App extends React.Component<{}, IAppState> {

  constructor(props: {}) {
    super(props);
    this.state = {
      theme: "",
      themeStyle: ThemeStyle.Light,
    }
  }

  public componentDidMount() {
    app.initialize();
    app.getContext().then((context) => {
      let theme = context.app.theme || "";
      this.updateTheme(theme);
      this.setState({
        theme: theme
      });
    });

    app.registerOnThemeChangeHandler((theme) => {
      this.updateTheme(theme);
      this.setState({
        theme: theme,
      }, () => {
        this.forceUpdate();
      });
    });
  }

  public setThemeComponent = () => {
    if (this.state.theme === "dark") {
      return (
        <FluentProvider theme={teamsDarkTheme}>
          <div className="darkContainer">
            {this.getAppDom()}
          </div>
        </FluentProvider>
      );
    }
    else if (this.state.theme === "contrast") {
      return (
        <FluentProvider theme={teamsHighContrastTheme}>
          <div className="highContrastContainer">
            {this.getAppDom()}
          </div>
        </FluentProvider>
      );
    } else {
      return (
        <FluentProvider theme={teamsLightTheme}>
          <div className="defaultContainer">
            {this.getAppDom()}
          </div>
        </FluentProvider>
      );
    }
  }

  private updateTheme = (theme: string) => {
    if (theme === "dark") {
      this.setState({
        themeStyle: ThemeStyle.Dark
      });
    } else if (theme === "contrast") {
      this.setState({
        themeStyle: ThemeStyle.HighContrast
      });
    } else {
      this.setState({
        themeStyle: ThemeStyle.Light
      });
    }
  }

  public getAppDom = () => {
    const context = getContext({
      baseFontSize: 10,
      style: this.state.themeStyle
    });
    return (
      <TeamsThemeContext.Provider value={context}>
        <Suspense fallback={<div></div>}>
          <div className="appContainer">
            <BrowserRouter>
              <Routes>
                  <Route path="/tab" element={<Tab/>}></Route>
              </Routes>
            </BrowserRouter>
          </div>
        </Suspense>
      </TeamsThemeContext.Provider>
    );
  }

  public render(): JSX.Element {
    return (
      <div>
        {this.setThemeComponent()}
      </div>
    );
  }
}

export default App;