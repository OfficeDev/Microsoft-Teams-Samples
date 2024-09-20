const esbuild = require('esbuild');

esbuild.build({
    entryPoints: ['server.js'],
    bundle: true,
    platform: 'node',
    outfile: 'dist/index.js',
    loader: {
        '.node': 'file'
    },
    external: [
        'keytar' 
    ]
})
    .then(() => {
        console.log(`Build succeeded.`);
    })
    .catch((e) => {
        console.error("Error building:", e.message);
        process.exit(1);
    });
