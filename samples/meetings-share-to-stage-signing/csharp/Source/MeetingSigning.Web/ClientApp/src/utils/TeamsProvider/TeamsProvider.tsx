import * as microsoftTeams from '@microsoft/teams-js';
import React, { useEffect, useMemo, useState } from 'react';
import {
  teamsV2Theme,
  teamsDarkV2Theme,
  teamsHighContrastTheme,
  Provider as FluentUiProvider,
  ThemeInput,
} from '@fluentui/react-northstar';

export interface TeamsProviderContext {
  context: microsoftTeams.app.Context;
  microsoftTeams: typeof microsoftTeams;
  initializePromise: Promise<unknown>;
  anonymousUserAccessToken?: string;
  setAnonymousUserAccessToken: (accessToken: string) => void;
}

// promise that doesn't resolve.
// eslint-disable-next-line @typescript-eslint/no-empty-function
const never = new Promise<void>((resolve) => {});

export const TeamsContext = React.createContext<TeamsProviderContext>({
  microsoftTeams,
  initializePromise: never,
  context: {
    app: {
      theme: 'default',
      locale: '',
      sessionId: '',
      host: {
        name: microsoftTeams.HostName.teams,
        clientType: microsoftTeams.HostClientType.desktop,
        sessionId: '',
      },
    },
    page: {
      id: '',
      frameContext: microsoftTeams.FrameContexts.content,
    },
  },
  setAnonymousUserAccessToken: (accessToken: string) => {
    console.log(accessToken);
  },
});

export interface TeamsProviderProps {
  microsoftTeams: typeof microsoftTeams;
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
  children,
}: TeamsProviderProps): JSX.Element {
  const [initialized, setInitialized] = useState<boolean>(false);
  const [context, setContext] = useState<microsoftTeams.app.Context>({
    app: {
      theme: 'default',
      locale: '',
      sessionId: '',
      host: {
        name: microsoftTeams.HostName.teams,
        clientType: microsoftTeams.HostClientType.desktop,
        sessionId: '',
      },
    },
    page: {
      id: '',
      // This is a bad default as it's a possible frameContext value, but in this app we do not use it
      frameContext: microsoftTeams.FrameContexts.remove,
    },
  });
  const initializePromise = useMemo(
    () =>
      Promise.race([
        microsoftTeams.app.initialize(),
        new Promise((_, reject) =>
          setTimeout(
            () =>
              reject('Failed to initialize connection with Microsoft Teams'),
            5000,
          ),
        ),
      ]),
    [microsoftTeams],
  );
  const [anonymousUserAccessToken, setAnonymousUserAccessToken] = useState<
    string | undefined
  >();

  useEffect(() => {
    initializePromise.then(() => {
      setInitialized(true);
      microsoftTeams.app.getContext().then((context) => {
        setContext(context);
      });
      microsoftTeams.app.registerOnThemeChangeHandler((nextTheme) => {
        setContext((current) => ({
          ...current,
          theme: nextTheme,
        }));
      });
    });
  }, [microsoftTeams, setContext, initializePromise]);

  const theme = getTheme(context.app.theme);

  if (!initialized) {
    return <div>Loading...</div>;
  }

  return (
    <TeamsContext.Provider
      value={{
        microsoftTeams,
        context,
        initializePromise,
        anonymousUserAccessToken,
        setAnonymousUserAccessToken,
      }}
    >
      <FluentUiProvider theme={theme}>{children}</FluentUiProvider>
    </TeamsContext.Provider>
  );
}
