import { ApiErrorCode } from "models";

const isError = (errorCode: ApiErrorCode, error?: Error | null): boolean =>
  (error && error.message.startsWith(errorCode)) ?? false;

export { isError };
