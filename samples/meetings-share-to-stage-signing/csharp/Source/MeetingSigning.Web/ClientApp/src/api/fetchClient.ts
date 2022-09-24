import { merge } from 'lodash';
import { ApiErrorResponse, ErrorCode } from 'models/ApiErrorResponse';
import * as microsoftTeams from '@microsoft/teams-js';

// This function is for callers where authentication is required.
// It makes a fetch client call with an AAD token.
export async function authFetch<T>(urlPath: string, init?: RequestInit) {
  const token = await microsoftTeams.authentication
    .getAuthToken()
    .catch((err) => {
      throw new Error(`Unable to get Auth token ${err}`);
    });

  const mergedInit = merge({}, init, {
    headers: {
      Authorization: `Bearer ${token}`,
      Accept: 'application/json, text/plain',
      'Content-Type': 'application/json;charset=UTF-8',
    },
  });
  return (await fetchClient(urlPath, mergedInit)) as T;
}

async function fetchClient(urlPath: string, mergedInit: RequestInit): Promise<any> {
  const response = await fetch(`/${urlPath}`, mergedInit);

  if (!response.ok) {
    const errorJson: ApiErrorResponse = await response.json();
    if (!errorJson) {
      throw new Error('Error fetching data.');
    }

    if (errorJson.ErrorCode === ErrorCode.AuthConsentRequired) {
      try {
        await microsoftTeams.authentication.authenticate({
          url: `${window.location.origin}/auth-start`,
          width: 600,
          height: 535,
        });

        console.log('Consent provided.');
        return await fetchClient(urlPath, mergedInit);
      } catch (error) {
        console.error("Failed to get consent: '" + error + "'");
      }
    }

    throw new Error(`${errorJson.ErrorCode}: ${errorJson.Message}`);
  }

  return await response.json();
}
