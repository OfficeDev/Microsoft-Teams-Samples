import { merge } from 'lodash';

type Fetch = typeof fetch;
export type FetchMiddleware = (
  fetch: Fetch,
) => (...args: Parameters<Fetch>) => ReturnType<Fetch>;

export function composeFetchMiddleware(
  middleware: readonly FetchMiddleware[],
): FetchMiddleware {
  return function (fetch: Fetch) {
    return middleware.reduce(
      (currentFetch, middleware) => middleware(currentFetch),
      fetch,
    );
  };
}

export function defaultHeaders(headers: HeadersInit): FetchMiddleware {
  return (fetch: Fetch) => async (
    input: RequestInfo,
    init?: RequestInit | undefined,
  ) => {
    const mergedInit = merge({}, init, {
      headers,
    });
    return fetch(input, mergedInit);
  };
}

export function bearerTokenMiddleware(
  tokenFn: () => Promise<string>,
): FetchMiddleware {
  return (fetch: Fetch) => async (
    input: RequestInfo,
    init?: RequestInit | undefined,
  ) => {
    const token = await tokenFn();
    const headers = merge({}, init?.headers, {
      Authorization: `Bearer ${token}`,
    });
    const mergedInit = merge({}, init, {
      credetials: 'same-origin',
      headers,
    });

    return fetch(input, mergedInit);
  };
}

export function throwOnFailureMiddleware(): FetchMiddleware {
  return (fetch: Fetch) => async (
    input: RequestInfo,
    init?: RequestInit | undefined,
  ) => {
    const response = await fetch(input, init);
    if (!response.ok) {
      throw new Error(
        `Request failed: ${response.status}: ${response.statusText}`,
      );
    }
    return response;
  };
}
interface RequestParameters<Dto, Model> {
  url: string;
  method?: string | undefined;
  body?: string | any | undefined;
  dtoCast?: (dto: Dto) => Model;
}

export function requestBuilder(
  fetch: Fetch,
): <Dto, Model>(
  options: RequestParameters<Dto, Model>,
) => Promise<typeof options['dtoCast'] extends undefined ? undefined : Model> {
  return async function executeRequest<Dto, Model>(
    options: RequestParameters<Dto, Model>,
  ) {
    const { url, body, method, dtoCast } = options;
    let convertedBody = undefined;
    if (typeof body === 'string') {
      convertedBody = body;
    } else if (body !== undefined) {
      convertedBody = JSON.stringify(body);
    }
    const response = await fetch(url, {
      method,
      body: convertedBody,
    });
    if (dtoCast) {
      const json = await response.json();
      // TS gets confused with the conditional return type within the promise
      // eslint-disable-next-line @typescript-eslint/no-explicit-any
      return dtoCast(json) as any;
    } else {
      return undefined;
    }
  };
}

export type Request = ReturnType<typeof requestBuilder>;
