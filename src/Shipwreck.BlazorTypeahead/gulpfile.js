/// <binding BeforeBuild='default' Clean='clean' />
var gulp = require("gulp");
var concat = require('gulp-concat');
var uglify = require('gulp-uglify');
var del = require('del');
var ts = require('gulp-typescript');

gulp.task('clean', function () {
    return del(['content/*.js']);
});
gulp.task('tsc', function () {
    return gulp.src(['scripts/Shim.ts']).pipe(ts({
        outFile: 'Shim.js'
    })).pipe(gulp.dest('scripts/'));
});
gulp.task('scripts', function () {
    return gulp.src([
        'node_modules/bootstrap-3-typeahead/bootstrap3-typeahead.min.js',
        'Scripts/Shim.js'
    ])
        .pipe(concat('bundle.js'))
        .pipe(uglify({
            output: {
                comments: /^!/
            }
        }))
        .pipe(gulp.dest('content/'));
});
gulp.task('default', gulp.series(['clean', 'tsc', 'scripts']));