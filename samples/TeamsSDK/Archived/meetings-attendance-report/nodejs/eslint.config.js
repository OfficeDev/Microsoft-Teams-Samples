module.exports = [
    {
        ignores: ['build/**', 'dist/**', 'node_modules/**']
    },
    {
        files: ['**/*.js'],
        languageOptions: {
            ecmaVersion: 'latest',
            sourceType: 'commonjs',
            globals: {
                __dirname: 'readonly',
                console: 'readonly',
                module: 'readonly',
                process: 'readonly',
                require: 'readonly'
            }
        },
        rules: {
            indent: ['error', 4],
            'no-return-await': 'off',
            semi: ['error', 'always'],
            'space-before-function-paren': ['error', {
                named: 'never',
                anonymous: 'never',
                asyncArrow: 'always'
            }],
            'template-curly-spacing': ['error', 'always']
        }
    }
];