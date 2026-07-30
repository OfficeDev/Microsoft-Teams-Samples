const { src, dest, series } = require('gulp');
const zip = require('gulp-zip');
const del = require('del');

function clean() {
    return del(['manifest/**/*']);
}

function generateManifest() {
    return src(['appManifest/color.png', 'appManifest/outline.png', 'appManifest/manifest.json'])
        .pipe(zip('helloworldapp.zip'))
        .pipe(dest('manifest'));
}

exports.clean = clean;
exports['generate-manifest'] = generateManifest;
exports.default = series(clean, generateManifest);