// Copyright (c) Microsoft Corporation
// All rights reserved.
//
// MIT License:
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// 
// 

var gulp = require('gulp');
var ts = require('gulp-typescript');
var tslint = require('gulp-tslint');
var del = require('del');
var server = require('gulp-develop-server');
var mocha = require('gulp-spawn-mocha');
var sourcemaps = require('gulp-sourcemaps');
var zip = require('gulp-zip');
var browserify = require('browserify');
var tsify = require("tsify");
var source = require('vinyl-source-stream');
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

var filesToWatch = ['**/*.ts', '!node_modules/**'];
var filesToLint = ['**/*.ts', '!src/typings/**', '!node_modules/**'];
var staticFiles = ['src/**/*.json', 'src/**/*.pug', '!src/manifest.json'];
var clientJS = 'src/TaskModuleTab.ts';
var bundledJS = 'bundle.js';
var msTeamsLib = './node_modules/@microsoft/teams-js/dist/MicrosoftTeams.min.js';

/**
 * Clean build output.
 */
gulp.task('clean', (done) => {
    del([
        'build/**/*',
        // Azure doesn't like it when we delete build/src
        '!build/src'
        // 'manifest/**/*'
    ]);
    done();
});

/**
 * Lint all TypeScript files.
 */
gulp.task('ts:lint', (done) => {
    if (!process.env.GLITCH_NO_LINT) {
        gulp.src(filesToLint)
            .pipe(tslint({
                formatter: 'verbose'
            }))
            .pipe(tslint.report({
                summarizeFailureOutput: true
            }));
        done();
    } else {
        done();
    }
});

/**
 * Compile TypeScript and include references to library.
 */
gulp.task('ts', gulp.series("clean", (done) => {
    var tsProject = ts.createProject('./tsconfig.json', {
        // Point to the specific typescript package we pull in, not a machine-installed one
        typescript: require('typescript'),
    });
    
    tsProject
        .src()
        .pipe(sourcemaps.init())
        .pipe(tsProject())
        .js
        .pipe(sourcemaps.write('.', { sourceRoot: function(file) { return file.cwd + '/build'; }}))
        .pipe(gulp.dest('build/src'));
    done();
}));

/**
 * Copy statics to build directory.
 */
gulp.task('statics:copy', gulp.series("clean", (done) => {
    gulp.src(staticFiles, { base: '.' })
        .pipe(gulp.dest('./build'));
    done();
}));

/**
 * Copy (generated) client TypeScript files to the /scripts directory
 */
gulp.task('client-js', gulp.series("ts", (done) => {
    var bundler = browserify({
        basedir: ".",
        entries: clientJS,
        ignoreMissing: true,
        debug: false
    });

    var bundle = function() {
        return bundler
            .plugin(tsify)
            .bundle()
            .on('.error', function() {})
            .pipe(source(bundledJS))
            .pipe(gulp.dest('./public/scripts'));
    };

    if (global.isWatching) {
        bundler = watchify(bundler);
        bundler.on('update', bundle)
    }

    bundle();
    done();
}));

/**
 * Build application.
 */
gulp.task('build', gulp.series("clean", "ts:lint", "ts", "client-js", "statics:copy"));

/**
 * Build manifest
 */
gulp.task('generate-manifest', (done) => {
    gulp.src(['./public/images/*_icon.png', 'src/manifest.json'])
        .pipe(zip('TaskModule.zip'))
        .pipe(gulp.dest('manifest'));
    done();
});

/**
 * Build debug version of the manifest - 
 */
gulp.task('generate-manifest-debug', (done) => {
    gulp.src(['./public/images/*_icon.png', 'manifest/debug/manifest.json'])
        .pipe(zip('TaskModuleDebug.zip'))
        .pipe(gulp.dest('manifest/debug'));
    done();
});

/**
 * Run tests.
 */
gulp.task('test', gulp.series("ts", "statics:copy", (done) => {
    gulp
        .src('build/test/' + options.specFilter + '.spec.js', {read: false})
        .pipe(mocha({cwd: 'build/src'}))
        .once('error', function () {
            process.exit(1);
        })
        .once('end', function () {
            process.exit();
        });
    done();
}));

/**
 * Package up app into a ZIP file for Azure deployment.
 */
gulp.task('package', gulp.series("build", (done) => {
    var packagePaths = [
        'build/**/*',
        'public/**/*',
        'web.config',
        'package.json',
        '**/node_modules/**',
        '!build/src/**/*.js.map', 
        '!build/test/**/*', 
        '!build/test', 
        '!build/src/typings/**/*'];

    //add exclusion patterns for all dev dependencies
    var packageJSON = JSON.parse(fs.readFileSync(path.join(__dirname, 'package.json'), 'utf8'));
    var devDeps = packageJSON.devDependencies;
    for (var propName in devDeps) {
        var excludePattern1 = '!**/node_modules/' + propName + '/**';
        var excludePattern2 = '!**/node_modules/' + propName;
        packagePaths.push(excludePattern1);
        packagePaths.push(excludePattern2);
    }

    gulp.src(packagePaths, { base: '.' })
        .pipe(zip(options.packageName))
        .pipe(gulp.dest(options.packagePath));
    done();
}));

gulp.task('server:start', gulp.series("build", (done) => {
    server.listen({path: 'build/src/app.js'}, function(error) {
        console.log(error);
    });
    done();
}));

gulp.task('server:restart', gulp.series("build", (done) => {
    server.restart();
    done();
}));

gulp.task('default', gulp.series("server:start", (done) => {
    gulp.watch(filesToWatch, ['server:restart']);
    done();
}));

gulp.task('default', gulp.series("clean", "generate-manifest", (done) => {
    console.log('Build completed. Output in manifest folder');
    done();
}));