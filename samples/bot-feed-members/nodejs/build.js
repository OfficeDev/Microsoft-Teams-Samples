const esbuild = require('esbuild');

esbuild.build({
    entryPoints: ['index.js'],
    bundle: true,
    platform: 'node',
    outfile: 'dist/index.js',
    external: [
        // External dependencies that shouldn't be bundled
        '@azure/identity',
        '@microsoft/teams.api',
        '@microsoft/teams.apps',
        '@microsoft/teams.common'
    ]
})
    .then((r) => {
        console.log(`Build succeeded.`);
    })
    .catch((e) => {
        console.log("Error building:", e.message);
        process.exit(1);
    });