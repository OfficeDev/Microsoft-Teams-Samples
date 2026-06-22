module.exports = [
    {
        ignores: ['node_modules/**', 'dist/**', 'public/*.min.js']
    },
    {
        files: ['**/*.js'],
        languageOptions: {
            ecmaVersion: 'latest',
            sourceType: 'commonjs'
        },
        rules: {
            semi: ['error', 'always'],
            indent: ['error', 4],
            'no-return-await': 'off',
            'space-before-function-paren': ['error', {
                named: 'never',
                anonymous: 'never',
                asyncArrow: 'always'
            }],
            'template-curly-spacing': ['error', 'always']
        }
    }
];
