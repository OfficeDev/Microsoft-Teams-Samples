module.exports = [
    {
        ignores: [
            'build/**',
            'dist/**',
            'appManifest/build/**'
        ]
    },
    {
        files: ['**/*.js'],
        languageOptions: {
            ecmaVersion: 2022,
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