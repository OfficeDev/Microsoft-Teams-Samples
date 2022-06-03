import { ApiErrorCode } from "models";

/// Checks if a request error is a specific ApiErrorCode
const isApiErrorCode = (errorCode: ApiErrorCode, error?: Error | null): boolean =>
  (error && error.message.startsWith(errorCode)) ?? false;

export { isApiErrorCode };
