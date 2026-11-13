# Verify the local GNU R / glmnet toolchain before generating golden data.
# Usage: Rscript check_version.R
suppressPackageStartupMessages(library(glmnet))

v <- as.character(utils::packageVersion("glmnet"))
cat("R:      ", R.version.string, "\n", sep = "")
cat("glmnet: ", v, "\n", sep = "")

if (!identical(substr(v, 1, 3), "5.1")) {
    stop("glmnet 5.1 is required (the golden data were calibrated against 5.1)")
}

fml <- formals(glmnet)
cat("alpha default:            ", deparse(fml$alpha), "\n", sep = "")
cat("nlambda default:          ", deparse(fml$nlambda), "\n", sep = "")
cat("lambda.min.ratio default: ", deparse(fml$lambda.min.ratio), "\n", sep = "")
cat("cox.ties default:         ", paste(deparse(fml$cox.ties), collapse = " "), "\n", sep = "")
cat("type.multinomial default: ", paste(deparse(fml$type.multinomial), collapse = " "), "\n", sep = "")

datasets <- c("QuickStartExample", "BinomialExample", "PoissonExample",
              "MultinomialExample", "CoxExample", "MultiGaussianExample")
for (d in datasets) {
    ok <- d %in% utils::data(package = "glmnet")$results[, "Item"]
    cat(sprintf("  dataset %-22s %s\n", d, if (ok) "OK" else "MISSING"))
    if (!ok) stop(sprintf("required glmnet dataset '%s' is not available", d))
}

cat("OK: toolchain is ready\n")
