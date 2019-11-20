/// <binding AfterBuild='default' Clean='clean' />
var gulp = require("gulp");
var del = require('del');

gulp.task('clean', function () {
    return del(['wwwroot/js', 'wwwroot/css']);
});
gulp.task('jquery', function () {
    return gulp.src(['node_modules/jquery/dist/jquery.*'])
        .pipe(gulp.dest('wwwroot/js'));
});
gulp.task('bootstrap', function () {
    return gulp.src(['node_modules/bootstrap/dist/**/*'])
        .pipe(gulp.dest('wwwroot'));
});
gulp.task('default', gulp.series(['clean', 'jquery', 'bootstrap']));