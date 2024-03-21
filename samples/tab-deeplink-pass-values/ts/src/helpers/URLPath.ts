import $ from "jquery";

/**
 * This regular expression matches the following patterns:
 * - /deeplinkstab/
 * - /deeplinkstab/123
 *
 * In the example above, "123" is the value being passed by the deep link.
 * The passed value can be obtained with String#match(REGEXP)[2].
 * This regular expression is case insensitive.
 */
const REGEXP = /^\/deeplinkstab\/(([a-z0-9]+)\/?)?$/i;

/**
 * Helper function that retrieves and returns only the path portion from a full URL
 * @returns {string} URL path
 */
const getUrlPath = (): string => {
    return $(location).attr("pathname");
};

/**
 * Helper function that returns the values obtained from URL path based on regex
 * @param {string} urlPath URL path
 * @returns {RegExpMatchArray | null} Regular expression's backreference match results
 */
const getUrlMatch = (urlPath: string): string[] | null => {
    return urlPath.match(REGEXP);
};

/**
 * Returns the value passed from the deep link, if it's available in the URL path
 * @param {string} urlPath URL path
 * @returns {string | undefined} passed value or undefined
 */
export const getPassedValue = (urlPath: string = getUrlPath()): string | undefined => {
    const match = getUrlMatch(urlPath);
    if (match && match[2]) {
        return match[2];
    } else {
        return undefined;
    }
};
