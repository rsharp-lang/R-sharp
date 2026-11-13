# R# demo: predict.glmnet() / coef.glmnet() / deviance.glmnet().
#
# Run:   "G:\GCModeller\src\R-sharp\App\net10.0\R#.exe" demo_predict.R
# Input: data/*.csv   (produced by R/export_golden.R)
# Output: result/<family>_coef.csv, result/<family>_predict.csv
imports "glmnet" from "Rlapack";

const data_dir = file.path(@dir, "data");
const result_dir = file.path(@dir, "result");

const run_predict = function(fam, extras) {
    let x = read.csv(file.path(data_dir, `${fam}_x.csv`), check.names = FALSE);
    let y = read.csv(file.path(data_dir, `${fam}_y.csv`), check.names = FALSE);
    let golden = read.csv(file.path(data_dir, `${fam}_fit.csv`), check.names = FALSE);
    let lam = as.numeric(golden[, "lambda"]);
    let sdf = read.csv(file.path(data_dir, `${fam}_s.csv`));
    let s = as.numeric(sdf[, "s"]);
    let newx = read.csv(file.path(data_dir, `${fam}_newx.csv`), check.names = FALSE);

    let fit = glmnet::glmnet(x, y, family = fam, lambda = lam);
    let co = glmnet::coef.glmnet(fit, s = s);
    let pred = glmnet::predict.glmnet(fit, newx = newx, type = "link", s = s);

    write.csv(co,
              file = file.path(result_dir, `${fam}_coef.csv`),
              row.names = FALSE);
    write.csv(pred$values,
              file = file.path(result_dir, `${fam}_predict.csv`),
              row.names = FALSE);

    let msg = glmnet::print.glmnet(fit);
    print(`[${fam}] ${msg}`);

    if (extras) {
        # deviance.glmnet, response / nonzero prediction, exact refit and
        # the "lambda.min" / "lambda.1se" selectors are exercised once on the
        # gaussian example to keep the demo fast.
        let dev = glmnet::deviance.glmnet(fit, newx = x, newy = y, s = s);
        print(`  deviance = ${dev}`);

        let resp = glmnet::predict.glmnet(fit, newx = newx, type = "response", s = s);
        print(resp$values);

        let nz = glmnet::predict.glmnet(fit, newx = newx, type = "nonzero", s = s);
        print(nz$indices);

        let exact_co = glmnet::coef.glmnet(fit, s = s, exact = TRUE);
        print(exact_co);

        let fdf = read.csv(file.path(data_dir, `${fam}_foldid.csv`));
        let foldid = as.numeric(fdf[, "foldid"]);
        let cv = glmnet::cv.glmnet(x, y, family = fam, lambda = lam,
                                   foldid = foldid, nfolds = 5, type.measure = "mse");
        let cmin = glmnet::coef.glmnet(cv, s = "lambda.min");
        print(cmin);
        let c1se = glmnet::coef.glmnet(cv, s = "lambda.1se");
        print(c1se);

        let sm = glmnet::summary.glmnet(cv);
        print(sm$lambda.min);
        print(length(cv$lambda));
        print(cv$name);
    }
};

run_predict("gaussian", TRUE);
run_predict("binomial", FALSE);
run_predict("poisson", FALSE);
run_predict("multinomial", FALSE);
run_predict("cox", FALSE);
run_predict("mgaussian", FALSE);

print("OK: demo_predict.R finished");
