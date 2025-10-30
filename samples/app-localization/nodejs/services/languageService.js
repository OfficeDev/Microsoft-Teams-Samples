// Returns Response in the selected Teams language or defaults to English if not found
const GetTranslatedRes = (languageCode) => {
    try {
        // Try to load the translation file for the provided language code
        return require(`../translations/${languageCode}/common.json`);
    } catch (error) {
        // Log the error if the specified translation file is not found
        console.error(`Translation file for '${languageCode}' not found. Falling back to 'en-us'. Error: ${error.message}`);

        // Fallback to the default English translation
        return require(`../translations/en-us/common.json`);
    }
};

module.exports = {
    GetTranslatedRes
};