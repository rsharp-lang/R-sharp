# R# demo: glmnet() regularization paths for all six GNU R glmnet families.
#
# Run:   "G:\GCModeller\src\R-sharp\App\net10.0\R#.exe" demo_fit.R
# Input: data/*.csv   (produced by R/export_golden.R)
# Output: result/<family>_fit.csv, result/<family>_beta.csv
imports "glmnet" from "Rlapack";

const data_dir = file.path(@dir, "data");
const result_dir = file.path(@dir, "result");

const run_fit = function(fam) {
    let x = read.csv(file.path(data_dir, `${fam}_x.csv`), check.names = FALSE);
    let y = read.csv(file.path(data_dir, `${fam}_y.csv`), check.names = FALSE);
    let golden = read.csv(file.path(data_dir, `${fam}_fit.csv`), check.names = FALSE);
    let lam = as.numeric(golden[, "lambda"]);

    # Feed the exact GNU R lambda grid: the kernel truncates auto-generated
    # grids with the fdev early-stop rule, which would shift step indices.
    let fit = glmnet::glmnet(x, y, family = fam, lambda = lam);

    let msg = glmnet::print.glmnet(fit);
    print(`[${fam}] ${msg}`);

    # R-style result field access.
    let nlambda = length(fit$lambda);
    let ndf = length(fit$df);
    print(`  lambda steps = ${nlambda}, df steps = ${ndf}`);
    print(fit$family);
    print(fit$nobs);
    print(fit$nvars);
    let sm = glmnet::summary.glmnet(fit);
    print(sm$nlambda);

    write.csv(glmnet::as.data.frame.glmnet(fit),
              file = file.path(result_dir, `${fam}_fit.csv`),
              row.names = FALSE);
    write.csv(glmnet::glmnet.beta(fit),
              file = file.path(result_dir, `${fam}_beta.csv`),
              row.names = FALSE);
};

run_fit("gaussian");
run_fit("binomial");
run_fit("poisson");
run_fit("multinomial");
run_fit("cox");
run_fit("mgaussian");

print("OK: demo_fit.R finished");
