var gulp = require('gulp');
var ts = require('gulp-typescript');
var tslint = require('gulp-tslint');
var del = require('del');
var server = require('gulp-develop-server');
var sourcemaps = require('gulp-sourcemaps');
var zip = require('gulp-zip');
var path = require('path');
var minimist = require('minimist');
var fs = require('fs');
var _ = require('lodash');

var knownOptions = {
	string: 'packageName',
	string: 'packagePath',
	string: 'specFilter',
	default: {packageName: 'Package.zip', packagePath: path.join(__dirname, '_package'), specFilter: '*'}
};
var options = minimist(process.argv.slice(2), knownOptions);

var tsProject = ts.createProject('./tsconfig.json', {
    // Point to the specific typescript package we pull in, not a machine-installed one
    typescript: require('typescript'),
});

var filesToWatch = ['**/*.ts', '!node_modules/**'];
var filesToLint = ['**/*.ts', '!src/typings/**', '!node_modules/**'];
var staticFiles = ['**/*.json', '**/*.hbs'];

/**
 * Clean build output.
 */
gulp.task('clean', function () {
    return del([
        'build/**/*',
        // Azure doesn't like it when we delete build/src
        '!build/src'
    ]);
});

/**
 * Lint all TypeScript files.
 */
gulp.task('ts:lint', function () {
    if (!process.env.GLITCH_NO_LINT) {
        return gulp
            .src(filesToLint)
            .pipe(tslint({
                formatter: 'verbose'
            }))
            .pipe(tslint.report({
                summarizeFailureOutput: true
            }));
      }
});

/**
 * Compile TypeScript and include references to library.
 */
gulp.task('ts', function() {
    return tsProject
        .src()
        .pipe(sourcemaps.init())
        .pipe(tsProject())
        .pipe(sourcemaps.write('.', {includeContent: false, sourceRoot: '.'}))
        .pipe(gulp.dest('build'));
});

/**
 * Copy statics to build directory.
 */
gulp.task('statics:copy', function () {
    return gulp.src(staticFiles, { base: 'src' })
        .pipe(gulp.dest('./build'));
});

/**
 * Build application.
 */
gulp.task('build', gulp.parallel('ts:lint', 'ts', 'statics:copy'));
gulp.task('rebuild', gulp.series('clean', gulp.parallel('ts:lint', 'ts', 'statics:copy')));

/**
 * Build manifest
 */
gulp.task('generate-manifest', function() {
    gulp.src(['./manifest/*.png', 'manifest/manifest.json'])
        .pipe(zip('AuthBot.zip'))
        .pipe(gulp.dest('manifest'));
});

/**
 * Package up app into a ZIP file for Azure deployment.
 */
gulp.task('package', gulp.series('rebuild', function () {
    var packagePaths = [
        'build/**/*',
        'public/**/*',
        'web.config',
        'package.json',
        '**/node_modules/**',
        '!build/**/*.js.map'];

    //add exclusion patterns for all dev dependencies
    var packageJSON = JSON.parse(fs.readFileSync(path.join(__dirname, 'package.json'), 'utf8'));
    var devDeps = packageJSON.devDependencies;
    for (var propName in devDeps) {
        var excludePattern1 = '!**/node_modules/' + propName + '/**';
        var excludePattern2 = '!**/node_modules/' + propName;
        packagePaths.push(excludePattern1);
        packagePaths.push(excludePattern2);
    }

    return gulp.src(packagePaths, { base: '.' })
        .pipe(zip(options.packageName))
        .pipe(gulp.dest(options.packagePath));
}));

gulp.task('server:start', gulp.series('build', function() {
    server.listen({path: 'app.js', cwd: 'build/src'}, function(error) {
        console.error(error);
    });
}));

gulp.task('server:restart', gulp.series('build', function() {
    server.restart();
}));

gulp.task('default', gulp.series('server:start', function() {
    gulp.watch(filesToWatch, ['server:restart']);
}));
