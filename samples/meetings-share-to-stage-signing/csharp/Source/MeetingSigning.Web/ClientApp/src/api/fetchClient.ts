import { merge } from 'lodash';
import * as microsoftTeams from '@microsoft/teams-js';

/// This is a wrapper around teamsAuthFetch and fetchWithToken below,
/// deciding which one to use based on the isAnonymousUser flag.
export async function authFetch<T>(
  urlPath: string,
  isAnonymousUser: boolean,
  init?: RequestInit,
  anonToken?: string,
) {
  if (isAnonymousUser) {
    if (!anonToken) {
      throw new Error('You need to Sign in with Microsoft Account.');
    }

    return await fetchWithToken<T>(urlPath, anonToken, init);
  }

  return await teamsAuthFetch<T>(urlPath, init);
}

// This function is for callers where authentication is required.
// It makes a fetch client call with an AAD token.
async function teamsAuthFetch<T>(urlPath: string, init?: RequestInit) {
  const token = await microsoftTeams.authentication
    .getAuthToken()
    .catch((err) => {
      throw new Error(`Unable to get Auth token ${err}`);
    });

  return (await fetchWithToken(urlPath, token, init)) as T;
}

async function fetchWithToken<T>(
  urlPath: string,
  token: string,
  init?: RequestInit,
) {
  const mergedInit = merge({}, init, {
    headers: {
      Authorization: `Bearer ${token}`,
      Accept: 'application/json, text/plain',
      'Content-Type': 'application/json;charset=UTF-8',
    },
  });

  return (await fetchClient(urlPath, mergedInit)) as T;
}

async function fetchClient(
  urlPath: string,
  mergedInit: RequestInit,
): Promise<any> {
  const response = await fetch(`/${urlPath}`, mergedInit);

  if (!response.ok) {
    const errorJson: any = await response.json();
    if (!errorJson) {
      throw new Error('Error fetching data.');
    }

    if (errorJson.title) {
      throw new Error(`${errorJson.title}`);
    }

    throw new Error(`${errorJson.ErrorCode}: ${errorJson.Message}`);
  }

  return await response.json();
}
