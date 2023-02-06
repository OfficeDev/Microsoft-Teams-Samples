import * as microsoftTeams from '@microsoft/teams-js';

/**
 * Component rendered when starting an auth prompt.
 * It gathers the required information before calling AAD
 */
export default function AuthStart() {
  microsoftTeams.app.initialize();

  // Get the tab context, and use the information to navigate to Azure AD login page
  microsoftTeams.app.getContext().then(async (context) => {
    // Generate random state string and store it, so we can verify it in the callback
    let state = window.self.crypto.randomUUID();
    localStorage.setItem('auth-state', state);
    localStorage.removeItem('codeVerifier');

    let tenantId = context.user.tenant.id; // Tenant ID of the logged in user
    let clientId = process.env.REACT_APP_AAD_CLIENT_ID;

    const queryParams = {
      tenant: tenantId,
      client_id: clientId,
      response_type: 'token',
      scope: 'https://graph.microsoft.com/.default',
      redirect_uri: `${window.location.origin}/auth-end`,
      nonce: window.self.crypto.randomUUID(),
      state: state,
      login_hint: context.user.loginHint,
      prompt: 'consent',
    };

    let authorizeEndpoint = `https://login.microsoftonline.com/${tenantId}/oauth2/v2.0/authorize?${toQueryString(
      queryParams,
    )}`;
    window.location.assign(authorizeEndpoint);
  });

  // Build query string from map of query parameter
  function toQueryString(queryParams) {
    let encodedQueryParams = [];
    for (let key in queryParams) {
      encodedQueryParams.push(key + '=' + encodeURIComponent(queryParams[key]));
    }
    return encodedQueryParams.join('&');
  }

  return (
    <>
      <p>
        To complete that request you need to authorize us to access your
        Microsoft Graph data. Redirecting
      </p>
    </>
  );
}
