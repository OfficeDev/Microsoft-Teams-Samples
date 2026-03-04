/**
 * Creates a deep link to pass a value to tab app
 * @param {string} appId App ID from manifest
 * @param {string} entityId Entity ID set when configuring tab app (see DeepLinksTabConfig.tsx)
 * @param {string} inputValue input value to pass
 * @param {string} channelId Channel ID for channel that tab app belongs to
 * @returns Deep link
 */
export const getDeepLinkForTab = (appId: string, entityId: string, inputValue: string, channelId: string): string => {
    // Pass value using subEntityId in Context
    const context = {
        subEntityId: inputValue,
        channelId
    };

    // Encode context
    const encodedContext = encodeURIComponent(JSON.stringify(context));

    // Create deep link
    return `https://teams.microsoft.com/l/entity/${appId}/${entityId}?context=${encodedContext}`;
};

/**
 * Creates a deep link to pass a value to standalone web app
 * @param {string} urlBase URL base
 * @param {string} inputValue input value to pass
 * @returns Deep link
 */
export const getDeepLinkForWeb = (urlBase: string, inputValue: string): string => {
    return `${urlBase}/${inputValue}`;
};
