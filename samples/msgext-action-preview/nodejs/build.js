const esbuild = require('esbuild');

esbuild.build({
    entryPoints: ['src/index.ts'],
    bundle: true,
    platform: 'node',
    target: 'node20',
    outfile: 'dist/index.js',
    format: 'cjs',
    external: [
        // Keep external dependencies as external since they'll be in node_modules
        '@azure/identity',
        '@microsoft/teams.api',
        '@microsoft/teams.apps',
        '@microsoft/teams.cards',
        '@microsoft/teams.common',
        '@microsoft/teams.dev',
        '@microsoft/teams.graph'
    ],
    loader: {
        '.ts': 'ts'
    },
    sourcemap: true
})
    .then((r) => {
        console.log(`Build succeeded.`);
    })
    .catch((e) => {
        console.log("Error building:", e.message);
        process.exit(1);
    });