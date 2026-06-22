// Import esbuild for bundling the application
const esbuild = require('esbuild');

// Start the build process
esbuild.build({
    entryPoints: ['index.js'],
    bundle: true,
    platform: 'node',
    outfile: 'dist/index.js',
    sourcemap: true,
    minify: true,
    target: 'node18',
})
    .then(() => {
        console.log('Build succeeded.');
    })
    .catch((error) => {
        console.error(`Error building: ${error.message}`);
        process.exit(1);
    });
