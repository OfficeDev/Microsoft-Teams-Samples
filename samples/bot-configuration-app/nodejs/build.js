// Simple build script for the bot configuration app
const fs = require('fs');
const path = require('path');

// Check if all required files exist
const requiredFiles = [
  'package.json',
  'app.js',
  'teamsBot.js', 
  'index.js',
  'config.js',
  'appPackage/manifest.json'
];

let allFilesExist = true;
requiredFiles.forEach(file => {
  if (!fs.existsSync(path.join(__dirname, file))) {
    console.error(`Missing required file: ${file}`);
    allFilesExist = false;
  }
});

if (!allFilesExist) {
  console.error("Build failed - missing required files");
  process.exit(1);
}