const fs = require('fs');
const path = require('path');
const archiver = require('archiver');

// Create build directory if it doesn't exist
const buildDir = path.join(__dirname, 'appManifest', 'build');
if (!fs.existsSync(buildDir)) {
    fs.mkdirSync(buildDir, { recursive: true });
}

// Read the manifest file
const manifestPath = path.join(__dirname, 'appManifest', 'manifest.json');
const manifest = JSON.parse(fs.readFileSync(manifestPath, 'utf8'));

// Replace environment variables in manifest
const envReplacements = {
    '{{TEAMS_APP_ID}}': process.env.TEAMS_APP_ID || 'placeholder-teams-app-id',
    '{{TAB_DOMAIN}}': process.env.TAB_DOMAIN || 'placeholder-domain',
    '{{AAD_APP_CLIENT_ID}}': process.env.AAD_APP_CLIENT_ID || 'placeholder-client-id',
    '{{TEAMSFX_ENV}}': process.env.TEAMSFX_ENV || 'local'
};

let manifestString = JSON.stringify(manifest, null, 2);
Object.entries(envReplacements).forEach(([placeholder, value]) => {
    manifestString = manifestString.replace(new RegExp(`\\$\\{\\{${placeholder.slice(2, -2)}\\}\\}`, 'g'), value);
});

// Write the processed manifest
const outputManifestPath = path.join(buildDir, `manifest.${process.env.TEAMSFX_ENV || 'local'}.json`);
fs.writeFileSync(outputManifestPath, manifestString);

// Create the zip file
const outputZipPath = path.join(buildDir, `appManifest.${process.env.TEAMSFX_ENV || 'local'}.zip`);
const output = fs.createWriteStream(outputZipPath);
const archive = archiver('zip', { zlib: { level: 9 } });

output.on('close', () => {
    console.log(`App package created: ${outputZipPath} (${archive.pointer()} total bytes)`);
});

archive.on('error', (err) => {
    throw err;
});

archive.pipe(output);

// Add manifest file
archive.file(outputManifestPath, { name: 'manifest.json' });

// Add all PNG files from appManifest directory
const appManifestDir = path.join(__dirname, 'appManifest');
const files = fs.readdirSync(appManifestDir);
files.forEach(file => {
    if (file.endsWith('.png')) {
        const filePath = path.join(appManifestDir, file);
        archive.file(filePath, { name: file });
        console.log(`Added icon: ${file}`);
    }
});

archive.finalize();