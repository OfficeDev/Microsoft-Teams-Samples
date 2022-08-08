import * as microsoftTeams from '@microsoft/teams-js';

/**
 * Component rendered when returning from a auth prompt.
 * It checks the response from AAD, and notifies Teams
 */
export function AuthEnd() {
  (function () {
    microsoftTeams.initialize();

    const hashParams = getHashParameters();
    if (hashParams['error']) {
      microsoftTeams.authentication.notifyFailure(hashParams['error']);
    } else if (hashParams['code']) {
      // Check the state parameter
      const expectedState = localStorage.getItem('auth-state');
      if (expectedState !== hashParams['state']) {
        microsoftTeams.authentication.notifyFailure('StateDoesNotMatch');
      } else {
        // State parameter matches, report success
        localStorage.removeItem('auth-state');
        microsoftTeams.authentication.notifySuccess(hashParams['code']);
      }
    } else {
      microsoftTeams.authentication.notifyFailure('NoCodeInResponse');
    }
  })();

  // Parse hash parameters into key-value pairs
  function getHashParameters() {
    let hashParams = {};
    window.location.search
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
