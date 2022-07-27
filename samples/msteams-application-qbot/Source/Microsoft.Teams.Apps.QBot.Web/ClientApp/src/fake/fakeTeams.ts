import * as microsoftTeams from '@microsoft/teams-js';

export const fakeTeamsSdk: typeof microsoftTeams = {
  ...microsoftTeams,
  initialize: (callback) => {
    setTimeout(() => callback && callback(), 3_000); //setting to 3000ms to represent usual delay
  },
  registerOnThemeChangeHandler: (handler) => {
    // let count = 0;
    // const themes = ['default', 'dark', 'highContrast'];
    // setInterval(() => {
    //   count = count + (1 % themes.length);
    //   const theme = themes[count] ?? 'default';
    //   handler(theme);
    // }, 5_000);
    //TODO: make this periodically change the theme?
  },
  getContext: (cb) => {
    cb({
      entityId: '',
      locale: 'en',
    });
  },
  executeDeepLink: (
    deepLink: string,
    onComplete?: (status: boolean, reason?: string | undefined) => void,
  ) => {
    console.log('Execute Deeplink: ', { deepLink });
    onComplete && onComplete(true);
  },
  appInitialization: {
    ...microsoftTeams.appInitialization,
    notifyAppLoaded: (): void => {
      console.log('App loaded!');
    },
  },
};
