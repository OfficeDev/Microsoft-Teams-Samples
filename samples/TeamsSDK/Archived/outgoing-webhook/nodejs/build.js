// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license.

const esbuild = require('esbuild');

async function build() {
    try {
        await esbuild.build({
            entryPoints: ['app.js'],
            bundle: true,
            platform: 'node',
            outfile: 'dist/index.js'
        });
        console.log('Build succeeded.');
    } catch (e) {
        console.error('Error building:', e.message);
        process.exit(1);
    }
}

build();