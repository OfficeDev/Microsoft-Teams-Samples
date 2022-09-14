import * as microsoftTeams from '@microsoft/teams-js';
import { Story } from '@storybook/react';
import { QueryClient, QueryClientProvider } from 'react-query';
import { TeamsProvider } from './TeamsProvider';

export type TeamsThemeProviderProps = {
  context: any;
  story: Story;
};

export const withTeamsThemeProvider = (
  props: TeamsThemeProviderProps,
): React.ReactElement => {
  const mockedMicrosoftTeams: typeof microsoftTeams = {
    ...microsoftTeams,
    app: {
      ...microsoftTeams.app,
      initialize: () => {
        return new Promise<void>((resolve) => {
          resolve();
        });
      },
      getContext: () => {
        return new Promise<microsoftTeams.app.Context>((resolve) =>
          resolve({
            app: {
              theme: props.context.globals.theme,
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
          }),
        );
      },
      registerOnThemeChangeHandler: (handler: (theme: string) => void) => {
        handler(props.context.globals.theme);
      },
    },
  };

  const queryClient = new QueryClient();

  return (
    <TeamsProvider microsoftTeams={mockedMicrosoftTeams}>
      <QueryClientProvider client={queryClient}>
        {<props.story />}
      </QueryClientProvider>
    </TeamsProvider>
  );
};
