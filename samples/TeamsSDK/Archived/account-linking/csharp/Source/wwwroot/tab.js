// The following are wrappers around Microsoft Teams libraries to make them play nicer
const teamsPromise = Promise.race([
  new Promise(resolve => microsoftTeams.app.initialize().then(() => resolve())),
  new Promise((resolve, reject) => setTimeout(() => reject('Failed to initialize connection with Microsoft Teams'), 250))]);

async function openAuthPopup(redirectUri) {
  let url = redirectUri instanceof URL ? redirectUri : new URL(redirectUri);
  // Since the OAuth partner might not allow the embedded webview popup
  // https://docs.microsoft.com/en-us/microsoftteams/platform/tabs/how-to/authentication/auth-oauth-provider
  url.searchParams.set('oauthRedirectMethod', '{oauthRedirectMethod}');
  url.searchParams.set('authId', '{authId}');
    return await new Promise((resolve, reject) => microsoftTeams.authentication.authenticate({
        url: url.toString(),
        isExternal: true,
        height: 500,
        width: 400
    }).then((result) => resolve(result))
        .catch((error) => reject(error)));
}

async function getAccessToken() {
  await teamsPromise;
  const accessToken = await new Promise((resolve, reject) => {
      microsoftTeams.authentication.getAuthToken()
          .then((result) => resolve(result))
          .catch((error) => reject(error));
  });
  return accessToken;
}
// quick implementation of https://datatracker.ietf.org/doc/html/rfc4648#section-5
// by converting standard btoa to url & filesystem safe encoding (replacing entries 62/63 from the encoding table)
// this is assuredly not the most performant implementation as it iterates over the source string 3x
// but it is easily verifiable
function base64UrlEncode(buffer)
{
  let base64 = btoa(buffer);
  return base64
    .replace(/\+/g, '-') // replace '+' with '-' 
    .replace(/\//g, '_') // replace '/' with '_'
    .replace(/=*$/, ''); // remove trailing =
}

async function generatePKCECodes()
{
  let codeVerifier = crypto.randomUUID();
  let textEncoder = new TextEncoder();
  let codeVerifierBytes = textEncoder.encode(codeVerifier);
  let codeChallengeBuffer = await crypto.subtle.digest("SHA-256", codeVerifierBytes);
  let codeChallengeBytes = Array.from(new Uint8Array(codeChallengeBuffer));
  let codeChallenge = base64UrlEncode(String.fromCharCode(...codeChallengeBytes));
  
  return {
    codeVerifier,
    codeChallenge
  }
}

async function runUserConsentFlow(authorizeUrl)
{
  authorizeUrl = authorizeUrl instanceof URL ? authorizeUrl : new URL(authorizeUrl);

  // we need to create PKCE code challenge / code verification to both ensure the
  // auth originated from this client and to ensure that the code cannot be claimed by someone else
  // who eavesdrops on the response url
  let { codeVerifier, codeChallenge } = await generatePKCECodes();

  authorizeUrl.searchParams.append('code_challenge', codeChallenge);
  authorizeUrl.searchParams.append('state', codeChallenge);
  const authResponse = await openAuthPopup(authorizeUrl);
  var {code, state} = JSON.parse(authResponse);
  if (state != codeChallenge)
  {
    // In practice you would want to alert the end-user to the threat & record this for audit 
    throw "Code verification failed, potential authentication hijacking detected";
  }

  // now claim the code
  accessToken = await getAccessToken();
  await fetch('/accountLinking/link', {
    method: 'PUT',
    headers: new Headers({
      authorization: `Bearer ${accessToken}`,
      'Content-Type': 'application/json'
    }),
    body: JSON.stringify({
      code_verifier: codeVerifier,
      code: code
    }),
  }); 
}

// The UI elements we use / modify as part of our tab
const logoutButton = document.getElementById('logout');
const loginButton = document.getElementById('login');
const content = document.getElementById('content')

async function onLogout() {
  const accessToken = await getAccessToken();

  // Issue the request to the backend to log out the user. 
  await fetch('/accountLinking/link', {
    method: 'DELETE',
    headers: new Headers({
      authorization: `Bearer ${accessToken}`
    })
  });

  // Update the UI to reflect that the user isn't logged in.
  logoutButton.disabled = true;
  loginButton.disabled = false;
  content.innerText = "Please log in to see starred repositories";
}

async function onLogin()
{
  const accessToken = await getAccessToken();

  let response = await fetch('/github/repositories', {
    method: 'GET',
    headers: new Headers({
      authorization: `Bearer ${accessToken}`
    })
  });

  if (response.status == 412) {
    var authResponse = await response.json();
    console.log("Need to do the partner auth", { authResponse });
    
    await runUserConsentFlow(authResponse.authorize_url);
    // once the partner auth has completed, the user is logged in, update the UI
    console.log("Finished partner auth, user is now logged in");
    

    // re-run the request now that we are authenticated
    response = await fetch('/github/repositories', {
      method: 'GET',
      headers: new Headers({
        authorization: `Bearer ${accessToken}`
      })
    });
  }
  // If we hit this point, the user is logged in successfully. 
  // update the UI accordingly 
  logoutButton.disabled = false;
  loginButton.disabled = true;

  // read the response content and populate the content
  var responseJson = await response.json();
  content.innerText = JSON.stringify(responseJson, null, 2);
}

async function main() {
  console.log("Partner auth sample started");
  logoutButton.addEventListener('click', onLogout);
  loginButton.addEventListener('click', onLogin);
  await onLogin();
}

main().catch(err => console.error(err));