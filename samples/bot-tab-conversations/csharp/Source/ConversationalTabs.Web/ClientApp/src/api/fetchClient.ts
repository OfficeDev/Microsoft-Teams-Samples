import { merge } from 'lodash';
import { ApiError, ApiErrorCode } from 'models';
import * as microsoftTeams from '@microsoft/teams-js';

// This function is for callers where authentication is required.
// It makes a fetch client call with an AAD token.
export async function authFetch<T>(
  urlPath: string,
  init?: RequestInit,
) {
  const token = await getAuthToken().catch((err) => {
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

// This function is for callers where no authentication is required.
// It makes a fetch client call with out an AAD token.
export async function unAuthFetch<T>(urlPath: string, init?: RequestInit) {
  const mergedInit = merge({}, init, {
    headers: {
      Accept: 'application/json, text/plain',
      'Content-Type': 'application/json;charset=UTF-8',
    },
  });
  return (await fetchClient(urlPath, mergedInit)) as T;
}

async function getAuthToken() {
  return new Promise<string>((resolve, reject) => {
    microsoftTeams.authentication.getAuthToken({
      successCallback: (token) => {
        resolve(token);
      },
      failureCallback: (reason) => {
        reject(reason);
      },
    });
  });
}

async function fetchClient(
  urlPath: string,
  mergedInit: RequestInit,
) {
  const response = await fetch(`/${urlPath}`, mergedInit);

  if (!response.ok) {
    const errorJson: ApiError = await response.json();
    if (!errorJson) {
      throw new Error('Error fetching data.');
    }

    if (errorJson.errorCode === ApiErrorCode.AuthConsentRequired) {
      microsoftTeams.authentication.authenticate({
        url: `${window.location.origin}/auth-start`,
        width: 600,
        height: 535,
        successCallback: async (result) => {
          console.log('Consent provided.');
          return await fetchClient(urlPath, mergedInit);
        },
        failureCallback: (error) => {
          console.error("Failed to get consent: '" + error + "'");
        },
      });
    }

    throw new Error(`${errorJson.errorCode}: ${errorJson.message}`);
  }
  return await response.json();
}
