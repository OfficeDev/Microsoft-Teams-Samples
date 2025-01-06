const esbuild = require('esbuild');
esbuild.build({
    entryPoints: ['index.js'],
    bundle: true,
    platform: 'node',
    outfile: 'dist/index.js'
})
    .then((r) => {
        console.log(`Build succeeded.`);
    })
    .catch((e) => {
        console.log("Build failed with error:", e.message);
        process.exit(1);
    });