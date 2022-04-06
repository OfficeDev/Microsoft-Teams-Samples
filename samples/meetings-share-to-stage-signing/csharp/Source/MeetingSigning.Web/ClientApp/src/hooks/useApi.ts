import { useState, useContext, useEffect, useCallback } from 'react';
import * as microsoftTeams from '@microsoft/teams-js';
import { AuthContext, AuthState } from 'utils/AuthProvider';
import { ApiErrorResponse, ErrorCode } from 'models';

enum RequestState {
  Unrequested,
  Pending,
  Resolved,
  Rejected,
}

/**
 * Creates a React Hook that wraps an API function, and allows it to be called.
 *
 * This centralises the fetching of AuthToken for those calls, preventing the need
 * to read the token in each component.
 *
 * @param apiFunction The API function to call, A token will be the first parameter passed.
 *
 * @returns an object that incudes the response `data`, any `error`, if the request is `loading`
 * and a function to make the api call.
 */
export const useApi = (
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  apiFunction: (token: string, ...args: any[]) => Promise<Response>,
) => {
  const { token, state, setState } = useContext(AuthContext);

  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  const [data, setData] = useState<any>();
  const [error, setError] = useState<string | undefined>(undefined);
  const [requestState, setRequestState] = useState<RequestState>(
    RequestState.Unrequested,
  );

  const [consentRequiredCalled, setConsentRequiredCalled] = useState(false);
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  const [args, setArgs] = useState<any[]>([]);

  /**
   * Makes the API request call. Will only call if the auth token exists.
   * If the auth token does not exist, once it becomes available the call
   * will be made
   *
   * @param functionArgs Parameters that will be passed into the [apiFunction].
   */
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  const request = async (...functionArgs: any[]) => {
    setArgs(functionArgs);

    if (state === AuthState.Resolved && token) {
      await callApi(token, ...functionArgs);
    } else {
      setError('We were unable to authenticate you. Retrying.');
    }
  };

  /**
   * Opens an Auth window looking for additional consent.
   *
   * @param callback Function that is called when the auth window returns
   */
  const promptForConsent = useCallback(
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    (callback: (token?: any, error?: string) => void) => {
      setState(AuthState.Pending);
      // Cause Teams to popup a window for consent
      microsoftTeams.authentication.authenticate({
        url: `${window.location.origin}/auth-start`,
        width: 600,
        height: 535,
        successCallback: (token) => {
          callback(token, undefined);
        },
        failureCallback: (error) => {
          callback(undefined, error);
        },
      });
    },
    [setState],
  );

  /**
   * Actual method that handles calling the API, including making the call,
   * handling the response/errors. And if requires prompts for any additional consent
   * required.
   */
  const callApi = useCallback(
    /*  eslint-disable @typescript-eslint/no-explicit-any, sonarjs/cognitive-complexity */
    async (token: string, ...args: any[]) => {
      try {
        const response = await apiFunction(token, ...args);

        if (response.ok) {
          setData(await response.json());
          setRequestState(RequestState.Resolved);
          setError(undefined);
        } else {
          const apiErrorResponse: ApiErrorResponse = await response.json();
          setRequestState(RequestState.Rejected);
          setError(apiErrorResponse.Message);

          if (
            response.status === 403 &&
            apiErrorResponse?.ErrorCode === ErrorCode.AuthConsentRequired &&
            !consentRequiredCalled
          ) {
            setState(AuthState.Rejected);
            setConsentRequiredCalled(true);

            promptForConsent(async (newToken, promptError) => {
              if (promptError) {
                setError(promptError);
              } else if (newToken) {
                // Now that we have consented to new scopes, call the API again.
                setRequestState(RequestState.Pending);
                await callApi(token, ...args);
              }
            });
          }
        }
      } catch (caughtError: any) {
        setRequestState(RequestState.Rejected);
        setError(caughtError || 'Unexpected Error!');
      }
    },
    /* eslint-enable */
    [apiFunction, consentRequiredCalled, promptForConsent, setState],
  );

  /**
   * When a token changes, make that API call and update the `data` value.
   */
  useEffect(() => {
    const callApiWithNewToken = async () => {
      if (
        requestState !== RequestState.Resolved &&
        state === AuthState.Resolved &&
        token
      ) {
        await callApi(token, ...args);
      }
    };
    callApiWithNewToken();
  }, [args, requestState, state, token, callApi]);

  return {
    data,
    error,
    request,
  };
};
