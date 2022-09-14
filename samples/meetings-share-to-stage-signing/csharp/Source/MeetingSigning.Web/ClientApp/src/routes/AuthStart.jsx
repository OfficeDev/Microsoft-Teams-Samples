import * as microsoftTeams from '@microsoft/teams-js';

function generateAndSetTokenState(prefix) {
  // Generate random state string and store it, so we can verify it in the callback
  let state = `${prefix}${window.self.crypto.randomUUID()}`;
  localStorage.setItem('auth-state', state);
  localStorage.removeItem('codeVerifier');
  return state;
}

const commonQueryParams = () => {
  const queryParams = new URLSearchParams();

  queryParams.append('redirect_uri', `${window.location.origin}/auth-end`);
  queryParams.append('nonce', window.self.crypto.randomUUID());
  queryParams.append('response_type', 'token');

  return queryParams;
};

/**
 * Component rendered when starting an auth prompt.
 * It gathers the required information before calling AAD
 */
export function AuthStartAad() {
  microsoftTeams.app.initialize();

  // Get the tab context, and use the information to navigate to Azure AD login page
  microsoftTeams.app.getContext().then(async (context) => {
    let tenantId = context.user.tenant.id; // Tenant ID of the logged in user

    const queryParams = commonQueryParams();
    queryParams.append('client_id', process.env.REACT_APP_AAD_CLIENT_ID);
    queryParams.append('state', generateAndSetTokenState());
    queryParams.append('login_hint', context.user.loginHint);
    queryParams.append('scope', 'https://graph.microsoft.com/.default');
    queryParams.append('prompt', 'consent');

    let authorizeEndpoint = `https://login.microsoftonline.com/${tenantId}/oauth2/v2.0/authorize?${queryParams.toString()}`;
    debugger;
    window.location.assign(authorizeEndpoint);
  });

  return (
    <p>
      To complete that request you need to authorize us to access your Microsoft
      Graph data. Redirecting...
    </p>
  );
}

/**
 * Component rendered when starting an auth prompt for getting a Microsoft Auth token
 * Used for Anonymous users
 *
 * We are using a different AAD App for anonymous users than in-tenant users. This is to better reflect a real
 * world implementation where a 3P provider might be used. If you prefer you could use the same AAD app for both.
 */
export function AuthStartMsa() {
  const queryParams = commonQueryParams();

  queryParams.append('client_id', process.env.REACT_APP_MSA_ONLY_CLIENT_ID);
  queryParams.append('state', generateAndSetTokenState('anon_'));
  queryParams.append('scope', process.env.REACT_APP_MSA_ONLY_SCOPE);

  // debugger;
  let authorizeEndpoint = `https://login.microsoftonline.com/common/oauth2/v2.0/authorize?${queryParams.toString()}`;
  window.location.assign(authorizeEndpoint);

  return (
    <p>
      To complete that request you need to authorize us to access your Microsoft
      account. Redirecting...
    </p>
  );
}
