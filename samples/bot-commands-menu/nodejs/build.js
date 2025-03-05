const esbuild = require('esbuild');

/**
 * Function to build the project using esbuild.
 * This bundles the source files and generates an optimized output for the Node.js platform.
 * 
 * @returns {Promise<void>} - Resolves when the build process is successful.
 */
const buildProject = async () => {
    try {
        // Trigger the build process using esbuild.
        await esbuild.build({
            entryPoints: ['index.js'],  // Entry point of the application
            bundle: true,               // Bundle all dependencies into a single file
            platform: 'node',           // Target platform is Node.js
            outfile: 'dist/index.js'    // Output directory and file for the bundled code
        });

        console.log('Build succeeded.');

    } catch (error) {
        // Log the error and exit the process with a non-zero exit code.
        console.error('Error building:', error.message);
        process.exit(1);
    }
};

// Call the buildProject function to initiate the build.
buildProject();
