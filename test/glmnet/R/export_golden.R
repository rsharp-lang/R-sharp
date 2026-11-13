# Golden data exporter for the R# glmnet wrapper comparison test.
#
# Usage:
#   Rscript export_golden.R <data-directory>
#
# Exports, for each of the six glmnet families:
#   *_x.csv        design matrix (header V1..Vp)
#   *_y.csv        response (header V1..Vk)
#   *_foldid.csv   explicit 1-based fold assignment (removes CV split noise)
#   *_fit.csv      step/lambda/a0/df/dev.ratio/nzero/nulldev
#   *_beta.csv     step + flattened coefficient matrix
#   *_cv.csv       step/lambda/cvm/cvsd/lambda.min/lambda.1se
#   *_s.csv        reference lambda used by coef()/predict()
#   *_newx.csv     first 10 rows of x, used as newx by coef()/predict()
#   *_coef.csv     coef(fit, s = s_ref)
#   *_predict.csv  predict(fit, newx, s = s_ref, type = "link")
suppressPackageStartupMessages(library(glmnet))

args <- commandArgs(trailingOnly = TRUE)
if (length(args) < 1) {
    stop("usage: Rscript export_golden.R <data-directory>")
}
out <- args[1]
dir.create(out, showWarnings = FALSE, recursive = TRUE)

v <- as.character(utils::packageVersion("glmnet"))
if (!identical(substr(v, 1, 3), "5.1")) {
    stop("glmnet 5.1 is required for golden data generation")
}

# write.table() otherwise truncates to getOption("digits") significant digits,
# which is far too coarse for a numeric comparison test.
fmt <- function(x) {
    if (is.numeric(x)) {
        x[!is.finite(x)] <- 0
        trimws(formatC(x, format = "g", digits = 17))
    } else {
        as.character(x)
    }
}

write_tbl <- function(path, df) {
    if (!is.data.frame(df)) {
        df <- as.data.frame(df, optional = TRUE, stringsAsFactors = FALSE)
    }
    df[] <- lapply(df, fmt)
    utils::write.table(df, file = file.path(out, path), sep = ",",
                       row.names = FALSE, col.names = TRUE,
                       quote = FALSE, na = "0")
}

write_mat <- function(path, m, prefix = "V") {
    m <- as.matrix(m)
    d <- as.data.frame(m, optional = TRUE, stringsAsFactors = FALSE)
    colnames(d) <- paste0(prefix, seq_len(ncol(d)))
    write_tbl(path, d)
}

path_of <- function(prefix, suffix) paste0(prefix, "_", suffix, ".csv")

extract_beta <- function(fit, family) {
    b <- fit$beta
    if (is.list(b)) {
        mats <- lapply(b, function(m) t(as.matrix(m)))
    } else {
        mats <- list(t(as.matrix(b)))
    }
    do.call(cbind, mats)
}

extract_a0 <- function(fit, family, L) {
    # Only the single-response families expose a comparable scalar intercept.
    if (family %in% c("gaussian", "binomial", "poisson")) {
        as.vector(fit$a0)
    } else {
        rep(0, L)
    }
}

extract_coef <- function(fit, s) {
    co <- coef(fit, s = s)
    if (is.list(co)) {
        do.call(cbind, lapply(co, function(m) as.matrix(m)))
    } else {
        as.matrix(co)
    }
}

extract_link <- function(fit, newx, s) {
    pr <- predict(fit, newx = newx, s = s, type = "link")
    if (length(dim(pr)) == 3) {
        pr <- pr[, , 1]
    } else if (is.list(pr)) {
        pr <- do.call(cbind, lapply(pr, function(m) as.matrix(m)))
    }
    as.matrix(pr)
}

process <- function(prefix, family, x, y, measure) {
    x <- as.matrix(x)
    y <- if (is.null(dim(y))) as.matrix(y) else as.matrix(y)
    n <- nrow(x)
    foldid <- sample(rep(seq_len(5), length.out = n))

    write_mat(path_of(prefix, "x"), x)
    write_mat(path_of(prefix, "y"), y)
    write_tbl(path_of(prefix, "foldid"), data.frame(foldid = foldid))

    fit <- glmnet(x, y, family = family)
    lam <- as.vector(fit$lambda)
    L <- length(lam)

    beta <- extract_beta(fit, family)
    nzero <- as.integer(rowSums(abs(beta) > 1e-12))
    a0 <- extract_a0(fit, family, L)

    write_tbl(path_of(prefix, "fit"), data.frame(
        step = seq_len(L) - 1,
        lambda = lam,
        a0 = a0,
        df = as.vector(fit$df),
        dev.ratio = as.vector(fit$dev.ratio),
        nzero = nzero,
        nulldev = rep(fit$nulldev, L)
    ))

    beta_tbl <- cbind(data.frame(step = seq_len(L) - 1),
                      as.data.frame(beta, optional = TRUE))
    colnames(beta_tbl)[-1] <- paste0("V", seq_len(ncol(beta)))
    write_tbl(path_of(prefix, "beta"), beta_tbl)

    cv <- cv.glmnet(x, y, family = family, type.measure = measure,
                    foldid = foldid, lambda = lam)
    write_tbl(path_of(prefix, "cv"), data.frame(
        step = seq_along(cv$lambda) - 1,
        lambda = as.vector(cv$lambda),
        cvm = as.vector(cv$cvm),
        cvsd = as.vector(cv$cvsd),
        lambda.min = rep(cv$lambda.min, length(cv$lambda)),
        lambda.1se = rep(cv$lambda.1se, length(cv$lambda))
    ))

    # s_ref is an actual grid point (no interpolation) so R and R# evaluate
    # exactly the same model.
    s_ref <- lam[min(2, L)]
    write_tbl(path_of(prefix, "s"), data.frame(s = s_ref))

    newx <- x[seq_len(min(10, n)), , drop = FALSE]
    write_mat(path_of(prefix, "newx"), newx)

    write_mat(path_of(prefix, "coef"), extract_coef(fit, s_ref), prefix = "coef")
    write_mat(path_of(prefix, "predict"), extract_link(fit, newx, s_ref), prefix = "link")

    cat(sprintf("  %-12s family=%-12s n=%3d p=%3d L=%3d measure=%s\n",
                prefix, family, n, ncol(x), L, measure))
    invisible(NULL)
}

set.seed(1)
cat(sprintf("Exporting golden data to %s (glmnet %s)\n", normalizePath(out), v))

data(QuickStartExample)
process("gaussian", "gaussian", QuickStartExample$x, QuickStartExample$y, "mse")

data(BinomialExample)
process("binomial", "binomial", BinomialExample$x, BinomialExample$y, "deviance")

data(PoissonExample)
process("poisson", "poisson", PoissonExample$x, PoissonExample$y, "deviance")

data(MultinomialExample)
process("multinomial", "multinomial", MultinomialExample$x,
        as.integer(MultinomialExample$y), "mse")

data(CoxExample)
cox_y <- cbind(time = as.vector(CoxExample$y[, 1]), status = as.vector(CoxExample$y[, 2]))
process("cox", "cox", CoxExample$x, cox_y, "deviance")

data(MultiGaussianExample)
process("mgaussian", "mgaussian", MultiGaussianExample$x,
        as.matrix(MultiGaussianExample$y), "mse")

cat("OK: golden data exported\n")
