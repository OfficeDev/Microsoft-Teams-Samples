const js = require('@eslint/js');
const globals = require('globals');

module.exports = [
    {
        ignores: ['node_modules/**', 'dist/**', 'appManifest/build/**', 'build/**']
    },
    js.configs.recommended,
    {
        languageOptions: {
            ecmaVersion: 2022,
            sourceType: 'script',
            globals: {
                ...globals.node
            }
        },
        rules: {
            'no-unused-vars': ['warn', { argsIgnorePattern: '^_' }]
        }
    }
];
