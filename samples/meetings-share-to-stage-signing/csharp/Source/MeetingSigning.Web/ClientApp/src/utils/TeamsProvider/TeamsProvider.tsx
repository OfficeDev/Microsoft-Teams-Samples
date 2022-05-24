import * as microsoftTeams from '@microsoft/teams-js';
import { Context } from '@microsoft/teams-js';
import React, { useEffect, useMemo, useState } from 'react';
import {
  teamsV2Theme,
  teamsDarkV2Theme,
  teamsHighContrastTheme,
  Provider as FluentUiProvider,
  ThemeInput,
} from '@fluentui/react-northstar';

export interface TeamsProviderContext {
  context: Context;
  microsoftTeams: typeof microsoftTeams;
  initializePromise: Promise<unknown>;
  setContext: (ctx: Context) => void;
}

// promise that doesn't resolve.
// eslint-disable-next-line @typescript-eslint/no-empty-function
const never = new Promise<void>((resolve) => {});

export const TeamsContext = React.createContext<TeamsProviderContext>({
  microsoftTeams,
  initializePromise: never,
  context: {
    entityId: '',
    locale: '',
  },
  // eslint-disable-next-line @typescript-eslint/no-empty-function
  setContext: () => {},
});

export interface TeamsProviderProps {
  microsoftTeams: typeof microsoftTeams;
  initialContext?: Partial<Context>;
  children?: JSX.Element | JSX.Element[];
}

export const themeMap: { [key: string]: ThemeInput } = {
  dark: teamsDarkV2Theme,
  default: teamsV2Theme,
  contrast: teamsHighContrastTheme,
};

export function getTheme(theme?: string): ThemeInput {
  const key = theme && theme in themeMap ? theme : 'default';
  return themeMap[key];
}

export function TeamsProvider({
  microsoftTeams,
  initialContext,
  children,
}: TeamsProviderProps): JSX.Element {
  const contextInit: Context = {
    entityId: '',
    locale: 'en',
    ...initialContext,
  };
  const [context, setContext] = useState<Context>(contextInit);
  const initializePromise = useMemo(
    () =>
      Promise.race([
        new Promise<void>((resolve) => microsoftTeams.initialize(resolve)),
        new Promise((resolve, reject) =>
          setTimeout(
            () =>
              reject('Failed to initialize connection with Microsoft Teams'),
            1000,
          ),
        ),
      ]),
    [microsoftTeams],
  );

  useEffect(() => {
    async function registerHandlers() {
      await initializePromise;
      microsoftTeams.getContext(setContext);
      microsoftTeams.registerOnThemeChangeHandler((nextTheme) => {
        setContext((current) => ({
          ...current,
          theme: nextTheme,
        }));
      });
    }
    registerHandlers();
  }, [initializePromise, microsoftTeams, setContext]);

  const theme = getTheme(context.theme);

  return (
    <TeamsContext.Provider
      value={{ microsoftTeams, context, setContext, initializePromise }}
    >
      <FluentUiProvider theme={theme}>{children}</FluentUiProvider>
    </TeamsContext.Provider>
  );
}
