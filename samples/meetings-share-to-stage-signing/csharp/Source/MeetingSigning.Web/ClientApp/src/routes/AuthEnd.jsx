import * as microsoftTeams from '@microsoft/teams-js';
import { MsalAuth } from 'utils/MsalAuth';

/**
 * Component rendered when returning from a auth prompt.
 * It checks the response from AAD, and notifies Teams
 */
export function AuthEndAad() {
  (function () {
    microsoftTeams.app.initialize();

    const hashParams = getHashParameters();

    if (hashParams['error']) {
      microsoftTeams.authentication.notifyFailure(hashParams['error']);
    } else if (hashParams['access_token']) {
      // Check the state parameter
      const expectedState = localStorage.getItem('auth-state');
      if (expectedState !== hashParams['state']) {
        microsoftTeams.authentication.notifyFailure('StateDoesNotMatch');
      } else {
        // State parameter matches, report success
        localStorage.removeItem('auth-state');
        microsoftTeams.authentication.notifySuccess(hashParams['access_token']);
      }
    } else {
      microsoftTeams.authentication.notifyFailure('NoTokenInResponse');
    }
  })();

  // Parse hash parameters into key-value pairs
  function getHashParameters() {
    let hashParams = {};
    window.location.hash
      .substring(1)
      .split('&')
      .forEach(function (item) {
        let s = item.split('='),
          k = s[0],
          v = s[1] && decodeURIComponent(s[1]);
        hashParams[k] = v;
      });
    return hashParams;
  }

  return (
    <>
      <h1>Completing authentication...</h1>
    </>
  );
}

export function AuthEndMsa() {
  (function () {
    microsoftTeams.app.initialize();

    const msalAuth = new MsalAuth();
    msalAuth.loadAuthModule();
  })();

  return (
    <>
      <h1>Completing authentication...</h1>
    </>
  );
}
