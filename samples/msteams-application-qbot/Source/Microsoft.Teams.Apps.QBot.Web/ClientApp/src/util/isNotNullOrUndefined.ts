// checks if value is null or undefined
// used for filtering out null/undefined from lists in a type safe way
export function isNotNullOrUndefined<T>(
  input: T | undefined | null,
): input is T {
  return input !== undefined && input !== null;
}
