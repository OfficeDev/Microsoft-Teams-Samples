const js = require("@eslint/js");

module.exports = [
    js.configs.recommended,
    {
        rules: {
            "semi": ["error", "always"],
            "indent": ["error", 4]
        }
    }
];
