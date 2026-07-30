import esbuild from 'esbuild';

try {
    await esbuild.build({
        entryPoints: ['index.js'],
        bundle: true,
        platform: 'node',
        outfile: 'dist/index.js',
        format: 'esm',
    });
    console.log('Build succeeded.');
} catch (e) {
    console.error('Error building:', e.message);
    process.exit(1);
}