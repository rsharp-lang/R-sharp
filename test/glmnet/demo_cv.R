# R# demo: cv.glmnet() cross validation for all six families.
#
# Run:   "G:\GCModeller\src\R-sharp\App\net10.0\R#.exe" demo_cv.R
# Input: data/*.csv   (produced by R/export_golden.R)
# Output: result/<family>_cv.csv
imports "glmnet" from "Rlapack";

const data_dir = file.path(@dir, "data");
const result_dir = file.path(@dir, "result");

const run_cv = function(fam, measure) {
    let x = read.csv(file.path(data_dir, `${fam}_x.csv`), check.names = FALSE);
    let y = read.csv(file.path(data_dir, `${fam}_y.csv`), check.names = FALSE);
    let golden = read.csv(file.path(data_dir, `${fam}_fit.csv`), check.names = FALSE);
    let lam = as.numeric(golden[, "lambda"]);
    let fdf = read.csv(file.path(data_dir, `${fam}_foldid.csv`));
    let foldid = as.numeric(fdf[, "foldid"]);

    # The exported foldid is 1-based (GNU R); the wrapper converts it to the
    # 0-based convention used by the kernel. nfolds must match the number of
    # distinct folds, otherwise empty folds would pollute cvm/cvsd.
    let cv = glmnet::cv.glmnet(x, y, family = fam, lambda = lam,
                               foldid = foldid, nfolds = 5,
                               type.measure = measure);

    let msg = glmnet::print.glmnet(cv);
    print(`[${fam}] ${msg}`);

    # R-style result field access.
    let lmin = cv$lambda.min;
    let l1se = cv$lambda.1se;
    print(`  lambda.min = ${lmin}, lambda.1se = ${l1se}`);
    print(length(cv$cvm));
    print(length(cv$foldid));

    write.csv(glmnet::glmnet.cv(cv),
              file = file.path(result_dir, `${fam}_cv.csv`),
              row.names = FALSE);
    write.csv(glmnet::as.data.frame.glmnet(cv),
              file = file.path(result_dir, `${fam}_cv_frame.csv`),
              row.names = FALSE);
};

run_cv("gaussian", "mse");
run_cv("binomial", "deviance");
run_cv("poisson", "deviance");
run_cv("multinomial", "mse");
run_cv("cox", "deviance");
run_cv("mgaussian", "mse");

print("OK: demo_cv.R finished");
