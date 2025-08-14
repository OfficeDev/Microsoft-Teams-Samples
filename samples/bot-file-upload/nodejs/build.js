const esbuild = require('esbuild');

/**
 * Builds the project using esbuild with the specified configuration.
 */
async function buildProject() {
    try {
        await esbuild.build({
            entryPoints: ['index.js'],
            bundle: true,
            platform: 'node',
            outfile: 'dist/index.js'
        });
        console.log('Build succeeded! Output saved to dist/index.js');
    } catch (e) {
        console.error('Error building project:', e.message);
        console.error(e.stack);  // Log the stack trace for better debugging
        process.exit(1);
    }
}

// Call the build function
buildProject();
