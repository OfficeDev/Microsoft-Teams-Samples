import esbuild from 'esbuild';

esbuild.build({
    entryPoints: ['index.ts'],
    bundle: true,
    platform: 'node',
    outfile: 'dist/index.js'
})
    .then(() => {
        console.log('Build succeeded.');
    })
    .catch((e) => {
        console.log('Error building:', e.message);
        process.exit(1);
    });