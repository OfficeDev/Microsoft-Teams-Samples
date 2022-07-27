import * as microsoftTeams from '@microsoft/teams-js';
import { Context } from "@microsoft/teams-js";
import { Story } from "@storybook/react";
import { TeamsProvider } from "./TeamsProvider";

export type TeamsThemeProviderProps = {
  context: any;
  story: Story;
};

export const withTeamsThemeProvider = (
  props: TeamsThemeProviderProps,
): React.ReactElement => {
  let mockedMicrosoftTeams: typeof microsoftTeams = {
    ...microsoftTeams,
    initialize: (callback?: ((value: unknown) => void) | undefined) => {
      if (callback) {
        return callback('Done');
      }
    },
    getContext: (callback: (context: Context) => void) => {
      callback({
        entityId: '',
        locale: '',
        theme: 'default',
      });
    },
    registerOnThemeChangeHandler: (handler: (theme: string) => void) => {
      handler(props.context.globals.theme);
    },
  };

  return (
    <TeamsProvider microsoftTeams={mockedMicrosoftTeams}>
      {<props.story />}
    </TeamsProvider>
  );
};
