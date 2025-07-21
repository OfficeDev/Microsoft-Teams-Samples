// Import the esbuild module for bundling
const esbuild = require('esbuild');

/**
 * Function to build the project using esbuild.
 * This function handles the building process and logs the results.
 */
async function buildProject() {
    try {
        await esbuild.build({
            entryPoints: ['index.js'], // Specify the entry file
            bundle: true, // Bundle all dependencies
            platform: 'node', // Specify that it's for Node.js platform
            outfile: 'dist/index.js' // Output the bundled file
        });

        console.log('Build succeeded.'); // Log success message
    } catch (error) {
        // Log error details if the build fails
        console.error('Error building the project:', error.message);
        process.exit(1); // Exit the process with a failure code
    }
}

// Run the build project function
buildProject();
