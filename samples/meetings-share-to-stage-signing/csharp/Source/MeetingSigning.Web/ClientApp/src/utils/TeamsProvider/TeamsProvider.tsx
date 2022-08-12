import * as microsoftTeams from '@microsoft/teams-js';
import React, { useEffect, useMemo, useState } from 'react';
import {
  teamsV2Theme,
  teamsDarkV2Theme,
  teamsHighContrastTheme,
  Provider as FluentUiProvider,
  ThemeInput,
} from '@fluentui/react-northstar';
import { Story } from '@storybook/react';

export interface TeamsProviderContext {
  context: microsoftTeams.app.Context;
  microsoftTeams: typeof microsoftTeams;
  initializePromise: Promise<void>;
  setContext: (ctx: microsoftTeams.app.Context) => void;
}

// promise that doesn't resolve.
// eslint-disable-next-line @typescript-eslint/no-empty-function
const never = new Promise<void>((resolve) => {});

export const TeamsContext = React.createContext<TeamsProviderContext>({
  microsoftTeams,
  initializePromise: never,
  context: {} as microsoftTeams.app.Context,
  // eslint-disable-next-line @typescript-eslint/no-empty-function
  setContext: () => {},
});

export interface TeamsProviderProps {
  microsoftTeams: typeof microsoftTeams;
  initialContext?: Partial<microsoftTeams.app.Context>;
  children?: JSX.Element | JSX.Element[];
}

export const themeMap: { [key: string]: ThemeInput } = {
  dark: teamsDarkV2Theme,
  default: teamsV2Theme,
  contrast: teamsHighContrastTheme,
};

export function getTheme(theme?: string): ThemeInput {
  const key = theme && theme in themeMap ? theme : 'dark';
  return themeMap[key];
}

export function TeamsProvider({
  microsoftTeams,
  initialContext,
  children,
}: TeamsProviderProps): JSX.Element {
  const contextInit: any = {
    app: {
      locale: "en-us",
    },
    page: {
      id: ''
    },
    ...initialContext,
  };
  const [context, setContext] = useState<microsoftTeams.app.Context>(contextInit);
  const initializePromise = useMemo(
    () => new Promise<void>((resolve) => microsoftTeams.initialize(resolve)),
    [microsoftTeams],
  );

  useEffect(() => {
    async function registerHandlers() {
      await initializePromise;
      microsoftTeams.app.getContext().then((context) => setContext(context));
      microsoftTeams.registerOnThemeChangeHandler((nextTheme) => {
        setContext((current) => ({
          ...current,
          theme: nextTheme,
        }));
      });
    }
    registerHandlers();
  }, [initializePromise, microsoftTeams, setContext]);

  const theme = getTheme(context.app.theme);

  return (
    <TeamsContext.Provider
      value={{ microsoftTeams, context, setContext, initializePromise }}
    >
      <FluentUiProvider theme={theme}>{children}</FluentUiProvider>
    </TeamsContext.Provider>
  );
}

export type TeamsThemeProviderProps = {
  story: Story;
};

export const withTeamsThemeProvider = (
  props: TeamsThemeProviderProps,
): React.ReactElement => {
  return (
    <TeamsProvider microsoftTeams={microsoftTeams}>
      {<props.story />}
    </TeamsProvider>
  );
};
