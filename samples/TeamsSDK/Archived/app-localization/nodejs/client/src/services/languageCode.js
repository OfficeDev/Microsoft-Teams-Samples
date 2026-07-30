// Language utility module to convert Microsoft Language Code to a supported language format

// Converts MS Language Code to a supported format by replacing hyphens with underscores
// and checking against the predefined list of supported languages
const LangCode = (code) => {
    // List of supported language codes (as per the system requirements)
    const supported_langs = ['en_us', 'es_mx', 'hi_in'];

    // If no language code is provided, return the default language (English)
    if (!code) return supported_langs[0];

    // Replace hyphen with an underscore and convert the code to lowercase
    let lanCode = code.replace('-', '_').toLowerCase();

    // Check if the processed language code exists in the list of supported languages
    let lan_supported = supported_langs.find(l => l === lanCode);

    // If the language code is supported, return it; otherwise, return the default language (English)
    if (lan_supported)
        return lanCode;
    else 
        return supported_langs[0];
}

// Export the LangCode function for use in other parts of the application
module.exports = {
    LangCode
}
