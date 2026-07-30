const esbuild = require('esbuild');

esbuild.build({
    entryPoints: ['index.js'],
    bundle: true,
    platform: 'node',
    outfile: 'dist/index.js'
}).then(() => {
    console.log('Build succeeded.');
}).catch((error) => {
    console.error('Error building:', error.message);
    process.exit(1);
});
