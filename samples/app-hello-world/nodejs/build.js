const esbuild = require('esbuild');
esbuild.build({
    entryPoints: ['src/app.js'],
    bundle: true,
    platform: 'node',
    outfile: 'dist/index.js'
})
    .then((r) => {
        console.log(`Build succeeded.`);
    })
    .catch((error) => {
        console.error("Error building:", error.message);
        process.exit(1);
    });