import { merge } from 'lodash';

export default async function authFetch(
  token: string,
  urlPath: string,
  init?: RequestInit,
): Promise<Response> {
  const headers = createHeaders(token, init?.headers);

  const mergedInit = merge({}, init, {
    credentials: 'same-origin',
    headers,
  });

  return fetch(`/${urlPath}`, mergedInit);
}

export function createHeaders(
  token: string,
  initHeaders?: HeadersInit,
): HeadersInit {
  return merge({}, initHeaders, {
    Authorization: `Bearer ${token}`,
    Accept: 'application/json, text/plain',
    'Content-Type': 'application/json;charset=UTF-8',
  });
}
