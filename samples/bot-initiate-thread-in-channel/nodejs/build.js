// Import esbuild for bundling the application
const esbuild = require('esbuild');

// Start the build process
esbuild.build({
    entryPoints: ['index.js'],  // Entry point for the application
    bundle: true,                // Bundle all dependencies into the output file
    platform: 'node',            // Target platform for Node.js
    outfile: 'dist/index.js',    // Output file location
    sourcemap: true,             // Add sourcemaps for debugging
    minify: true,                // Minify the output file to reduce size
    target: 'node16',            // Ensure compatibility with Node.js version 16 (or adjust as needed)
})
    .then(() => {
        console.log('Build succeeded.');
    })
    .catch((error) => {
        console.error(`Error building: ${error.message}`);
        process.exit(1);
    });
