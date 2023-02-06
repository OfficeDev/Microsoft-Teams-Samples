import { ApiErrorCode } from 'models';
import { SetStateAction } from 'react';

/// Function to determine if React Query should retry a API call when an error is returned
/// The backend API attempts to get an on-behalf-off token for the newly granted scopes immediately after the user consents.
/// Rarely, but sometimes, AAD has not propagated the consent completely, so it returns a token without the new scopes.
/// This logic, ensures we retry the call a few seconds later instead of asking the user to re-consent for scopes they already granted.
const apiRetryQuery = (
  failureCount: number,
  error: Error,
  userHasConsented: boolean,
  setUserHasConsented: (value: SetStateAction<boolean>) => void,
): boolean => {
  if (failureCount < 3) {
    return (
      isApiErrorCode(ApiErrorCode.AuthConsentRequired, error) &&
      userHasConsented
    );
  }

  // If we've retried a bunch and OBO still isn't able to get a token, we might not have consent.
  // Try to get consent again and stop retrying.
  setUserHasConsented(false);
  return false;
};

/// Checks if a request error is a specific ApiErrorCode
const isApiErrorCode = (
  errorCode: ApiErrorCode,
  error?: Error | null,
): boolean => (error && error.message.startsWith(errorCode)) ?? false;

export { apiRetryQuery, isApiErrorCode };
