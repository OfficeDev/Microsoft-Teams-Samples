const esbuild = require('esbuild'); // Import the esbuild module

// Build configuration for esbuild
esbuild.build({
    entryPoints: ['index.js'],  // Entry file for the build process
    bundle: true,               // Bundle all dependencies into a single file
    platform: 'node',           // Target platform is Node.js
    outfile: 'dist/index.js'    // Output file path
})
    .then(() => {
        console.log(`Build succeeded.`); // Log success message
    })
    .catch((e) => {
        console.log("Error building:", e.message); // Log error message
        process.exit(1); // Exit the process with an error code
    });
