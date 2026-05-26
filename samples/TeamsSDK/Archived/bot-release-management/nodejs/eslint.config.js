const js = require('@eslint/js');
const globals = require('globals');

module.exports = [
    js.configs.recommended,
    {
        files: ['**/*.js'],
        languageOptions: {
            ecmaVersion: 'latest',
            sourceType: 'script',
            globals: {
                ...globals.node
            }
        }
    },
    {
        ignores: ['node_modules/**', 'dist/**', 'build/**']
    }
];
