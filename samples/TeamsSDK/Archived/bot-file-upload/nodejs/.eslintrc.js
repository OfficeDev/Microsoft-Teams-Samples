/* eslint-disable */
module.exports = {
    "env": {
        "node": true,
        "es2021": true
    },
    "extends": "standard",
    "parserOptions": {
        "ecmaVersion": "latest"
    },
    "ignorePatterns": ["dist/", "node_modules/", "build/"],
    "rules": {
        "semi": [2, "always"],
        "indent": [2, 4],
        "no-return-await": 0,
        "space-before-function-paren": [2, {
            "named": "never",
            "anonymous": "never",
            "asyncArrow": "always"
        }],
        "template-curly-spacing": [2, "always"]
    }
};