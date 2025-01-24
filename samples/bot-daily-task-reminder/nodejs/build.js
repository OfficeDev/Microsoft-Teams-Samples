// Import esbuild package for bundling JavaScript code
const esbuild = require('esbuild');

/**
 * Perform the build process for bundling JavaScript files.
 */
async function buildProject() {
    try {
        // Perform the build process
        const result = await esbuild.build({
            entryPoints: ['index.js'],  // Entry point for the build process
            bundle: true,  // Bundle all dependencies into a single file
            platform: 'node',  // Specify the platform for the build (Node.js environment)
            outfile: 'dist/index.js'  // Output file path where the bundled file will be saved
        });
        
        // Log success message
        console.log(`Build succeeded. Output file is saved to ${result.outputFiles[0].path}`);
    } catch (error) {
        // Log detailed error message
        console.error("Error building:", error.message);
        console.error(error.stack);  // Log stack trace for better debugging
        
        process.exit(1);  // Exit the process with a non-zero exit code
    }
}

// Call the build function
buildProject();
