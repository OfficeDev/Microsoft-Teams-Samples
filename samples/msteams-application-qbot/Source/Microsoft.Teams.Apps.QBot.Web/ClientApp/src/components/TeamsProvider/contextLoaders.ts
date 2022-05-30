import { Context } from '@microsoft/teams-js';

type ContextLoaderMap = {
  [P in keyof Context]: () => Context[P] | undefined;
};
const stringValue = (value?: string): string | undefined => value;
const booleanValue = (value?: string): boolean | undefined =>
  value ? Boolean(value) : undefined;

export const loaderMap: ContextLoaderMap = {
  entityId: stringValue,
  locale: stringValue,
  channelId: stringValue,
  channelName: stringValue,
  groupId: stringValue,
  chatId: stringValue,
  isFullScreen: booleanValue,
  isMultiWindow: booleanValue,
  isTeamArchived: booleanValue,
  theme: stringValue,
  loginHint: stringValue,
  userObjectId: stringValue,
  userPrincipalName: stringValue,
  parentMessageId: stringValue,
};

type ContextKeyMap = {
  [P in keyof Context]?: string;
};

export const defaultContextKeyMap: ContextKeyMap = {
  entityId: 'entityId',
  subEntityId: 'subEntityId',
  locale: 'locale',
  channelId: 'channelId',
  channelName: 'channelName',
  groupId: 'groupId',
  chatId: 'chatId',
  isFullScreen: 'isFullScreen',
  isMultiWindow: 'isMultiWindow',
  isTeamArchived: 'isTeamArchived',
  theme: 'theme',
  loginHint: 'loginHint',
  userObjectId: 'userObjectId',
  userPrincipalName: 'userPrincipalName',
  parentMessageId: 'parentMessageId',
};

function createParamGetter(queryString: string) {
  const queryParams = new URLSearchParams(queryString);
  return function getParam(paramName: string) {
    return queryParams.get(paramName) ?? undefined;
  };
}

export function loadContextFromQueryString(
  queryString: string,
  keyMap?: ContextKeyMap,
): Partial<Context> {
  const keyMapping = keyMap ?? defaultContextKeyMap;
  const getParam = createParamGetter(queryString);
  const entries = Object.entries(keyMapping).map(([contextKey, urlKey]) => {
    const value = urlKey ? getParam(urlKey) : undefined;
    const converter =
      loaderMap[contextKey as keyof Context] ??
      ((value: string | undefined): undefined => undefined);
    const convertedValue = converter(value);
    return [contextKey, convertedValue];
  });
  return Object.fromEntries(entries) as Partial<Context>;
}
