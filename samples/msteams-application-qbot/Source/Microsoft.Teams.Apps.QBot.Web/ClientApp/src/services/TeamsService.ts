import { Context } from '@microsoft/teams-js';
import * as microsoftTeams from '@microsoft/teams-js';

type TeamsTheme = 'default' | 'dark' | 'contrast';
type ThemeChangeHandler = (theme: TeamsTheme) => void;
export interface AuthTokenParameters {
  resources?: string[];
  claims?: string[];
  silent?: boolean;
}
// Using the I prefix because there isn't really a better name for 'TeamsService'
export interface ITeamsService {
  getContext: () => Promise<Context>;
  registerOnThemeChangeHandler: (
    changeHandler: ThemeChangeHandler,
  ) => Promise<void>;
  getAuthToken: (parameters?: AuthTokenParameters) => Promise<string>;
}
export class TeamsService implements ITeamsService {
  private initPromise: Promise<void>;
  constructor() {
    this.initPromise = new Promise((resolve) => {
      microsoftTeams.initialize(resolve);
    });
  }

  async getContext(): Promise<Context> {
    await this.initPromise;
    return await new Promise<Context>((resolve) => {
      microsoftTeams.getContext((teamsContext) => resolve(teamsContext));
    });
  }

  async registerOnThemeChangeHandler(
    changeHandler: ThemeChangeHandler,
  ): Promise<void> {
    await this.initPromise;
    microsoftTeams.registerOnThemeChangeHandler((themeString) => {
      //TODO(nibeauli): investigate if there should be out-of-bounds checking
      const theme = themeString as TeamsTheme;
      changeHandler(theme);
    });
  }

  async getAuthToken(parameters: AuthTokenParameters = {}): Promise<string> {
    await this.initPromise;
    return await new Promise<string>((resolve, reject) => {
      microsoftTeams.authentication.getAuthToken({
        ...parameters,
        failureCallback: (reason) => reject(reason),
        successCallback: (token) => resolve(token),
      });
    });
  }
}
