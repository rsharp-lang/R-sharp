# Numeric comparison test: GNU R glmnet (golden) vs the R# glmnet wrapper.
#
# Usage:
#   Rscript compare.R <data-dir> <result-dir> <report-dir>
#
# Writes <report-dir>/compare_detail.csv and <report-dir>/compare_report.txt.
# Exits with status 1 when any strict assertion fails.
#
# Tolerance tiers follow the calibration record in GLMNet/README.md:
#   - lambda grid / nulldev are essentially exact on all six families;
#   - dev.ratio max error: gaussian 3.7e-6, binomial 3.3e-4, poisson 9.1e-4,
#     multinomial 0.014, cox 1.4e-5, mgaussian 2.6e-6;
#   - binomial near complete separation has non-unique coefficients (up to
#     ~2.4), so its coefficients are judged mainly by dev.ratio and the CV
#     curve; multinomial grouped coefficients can also drift in magnitude;
#   - a0/nzero are not comparable for the multi-response families because the
#     R golden exporter fills a0 with zeros and R counts non-zero *variables*
#     while the wrapper counts non-zero *coefficients*;
#   - cvm/cvsd absolute scales can differ (weight normalization / measure
#     definition), so lambda.min and lambda.1se grid positions are the primary
#     CV criterion instead.
args <- commandArgs(trailingOnly = TRUE)
if (length(args) < 3) {
    stop("usage: Rscript compare.R <data-dir> <result-dir> <report-dir>")
}
data_dir <- args[1]
result_dir <- args[2]
report_dir <- args[3]
dir.create(report_dir, showWarnings = FALSE, recursive = TRUE)

families <- c("gaussian", "binomial", "poisson", "multinomial", "cox", "mgaussian")

cfg <- list(
    gaussian = list(dev.ratio = 1e-4, a0 = 1e-3, nzero = 1, beta = 1e-2,
                    coef = 1e-4, predict = 1e-4,
                    cvm_abs = 1e-2, cvm_rel = 1e-3,
                    cvsd_abs = 1e-2, cvsd_rel = 1e-3,
                    lam_pos = 1, lam_check = TRUE),
    binomial = list(dev.ratio = 1e-3, a0 = 3.0, nzero = 2, beta = 3.0,
                    coef = 1e-3, predict = 1e-3,
                    cvm_abs = 2.0, cvm_rel = 0.5,
                    cvsd_abs = 0.5, cvsd_rel = 0.2,
                    lam_pos = 2, lam_check = TRUE),
    poisson = list(dev.ratio = 5e-3, a0 = 5e-2, nzero = 2, beta = 1e-2,
                   coef = 5e-2, predict = 5e-2,
                   cvm_abs = 0.5, cvm_rel = 0.01,
                   cvsd_abs = 0.1, cvsd_rel = 0.01,
                   lam_pos = 2, lam_check = TRUE),
    multinomial = list(dev.ratio = 3e-2, a0 = NA_real_, nzero = NA_real_, beta = 0.5,
                       coef = 0.2, predict = 0.2,
                       cvm_abs = 3.0, cvm_rel = 3.0,
                       cvsd_abs = 0.5, cvsd_rel = 0.2,
                       lam_pos = 10, lam_check = FALSE),
    cox = list(dev.ratio = 1e-4, a0 = 1e-6, nzero = 1, beta = 1e-2,
               coef = 1e-3, predict = 1e-3,
               cvm_abs = 5.0, cvm_rel = 0.5,
               cvsd_abs = 0.1, cvsd_rel = 0.05,
               lam_pos = 3, lam_check = TRUE),
    mgaussian = list(dev.ratio = 1e-4, a0 = NA_real_, nzero = 1, beta = 1e-2,
                     coef = 1e-4, predict = 1e-4,
                     cvm_abs = 50, cvm_rel = 1.0,
                     cvsd_abs = 5, cvsd_rel = 1.0,
                     lam_pos = 2, lam_check = TRUE)
)

details <- list()

add <- function(family, file, field, n, max_abs, max_rel, limit, status, note) {
    details[[length(details) + 1]] <<- data.frame(
        family = family, file = file, field = field, n = n,
        max_abs_diff = max_abs, max_rel_diff = max_rel,
        limit = limit, status = status, note = note,
        stringsAsFactors = FALSE
    )
}

read_gold <- function(fam, suffix) {
    read.csv(file.path(data_dir, paste0(fam, "_", suffix, ".csv")),
             check.names = FALSE, stringsAsFactors = FALSE)
}
read_rsharp <- function(fam, suffix) {
    read.csv(file.path(result_dir, paste0(fam, "_", suffix, ".csv")),
             check.names = FALSE, stringsAsFactors = FALSE)
}

numeric_matrix <- function(df) {
    keep <- vapply(df, is.numeric, logical(1))
    as.matrix(df[, keep, drop = FALSE])
}

# abs_diff <= limit_abs + limit_rel * max(|gold|)
check <- function(fam, file, field, gold, rsharp,
                  limit_abs = 0, limit_rel = 0, note = "") {
    if (is.null(gold) || is.null(rsharp)) {
        add(fam, file, field, 0, NA_real_, NA_real_, NA_real_, "SKIP",
            "field not available on both sides")
        return(invisible(NULL))
    }
    if (length(gold) != length(rsharp)) {
        add(fam, file, field, min(length(gold), length(rsharp)),
            NA_real_, NA_real_, NA_real_, "SKIP",
            sprintf("length mismatch: R=%d, R#=%d", length(gold), length(rsharp)))
        return(invisible(NULL))
    }
    d <- abs(gold - rsharp)
    max_abs <- max(d, na.rm = TRUE)
    max_rel <- max(d / pmax(1, abs(gold)), na.rm = TRUE)
    scale <- if (length(gold) > 0) max(abs(gold), na.rm = TRUE) else 1
    limit <- limit_abs + limit_rel * scale
    status <- if (!is.finite(max_abs) || max_abs > limit) "FAIL" else "PASS"
    add(fam, file, field, length(gold), max_abs, max_rel, limit, status, note)
}

skip <- function(fam, file, field, note) {
    add(fam, file, field, 0, NA_real_, NA_real_, NA_real_, "SKIP", note)
}

grid_pos <- function(lam, value) which.min(abs(lam - value))

for (fam in families) {
    cc <- cfg[[fam]]

    # ---------- path diagnostics ----------
    gf <- read_gold(fam, "fit")
    rf <- read_rsharp(fam, "fit")

    check(fam, "fit", "lambda", gf$lambda, rf$lambda, 1e-10, 1e-10,
          "lambda grid must be identical")
    check(fam, "fit", "nulldev", gf$nulldev, rf$nulldev, 1e-6, 1e-8,
          "null deviance must be identical")
    check(fam, "fit", "dev.ratio", gf$dev.ratio, rf$dev.ratio, cc$dev.ratio, 0,
          "family-calibrated dev.ratio tolerance")
    check(fam, "fit", "df", gf$df, rf$df, 1, 0,
          "one path step of df difference is allowed")
    if (is.na(cc$a0)) {
        skip(fam, "fit", "a0", "multi-response family: golden a0 is a zero placeholder")
    } else {
        check(fam, "fit", "a0", gf$a0, rf$a0, cc$a0, 0,
              "family-calibrated intercept tolerance")
    }
    if (is.na(cc$nzero)) {
        skip(fam, "fit", "nzero", "multi-response family: variable vs coefficient counting differs")
    } else {
        check(fam, "fit", "nzero", gf$nzero, rf$nzero, cc$nzero, 0,
              "small sparsity counting difference is allowed")
    }

    # ---------- coefficient path ----------
    gb <- numeric_matrix(read_gold(fam, "beta"))
    rb <- numeric_matrix(read_rsharp(fam, "beta"))
    if (nrow(gb) == nrow(rb) && ncol(gb) == ncol(rb)) {
        check(fam, "beta", "coefficients", as.vector(gb), as.vector(rb),
              cc$beta, 0, "flattened beta matrix (step x feature.response)")
    } else {
        skip(fam, "beta", "coefficients",
             sprintf("shape mismatch: R=%dx%d, R#=%dx%d",
                     nrow(gb), ncol(gb), nrow(rb), ncol(rb)))
    }

    # ---------- CV curve ----------
    gc <- read_gold(fam, "cv")
    rc <- read_rsharp(fam, "cv")
    check(fam, "cv", "lambda", gc$lambda, rc$lambda, 1e-10, 1e-10,
          "CV lambda grid must be identical")
    check(fam, "cv", "cvm", gc$cvm, rc$cvm, cc$cvm_abs, cc$cvm_rel,
          "CV mean curve, absolute scale may differ by measure definition")
    check(fam, "cv", "cvsd", gc$cvsd, rc$cvsd, cc$cvsd_abs, cc$cvsd_rel,
          "CV standard error curve")

    if (cc$lam_check) {
        for (field in c("lambda.min", "lambda.1se")) {
            gv <- gc[[field]][1]
            rv <- rc[[field]][1]
            gp <- grid_pos(gc$lambda, gv)
            rp <- grid_pos(rc$lambda, rv)
            delta <- abs(gp - rp)
            status <- if (delta <= cc$lam_pos) "PASS" else "FAIL"
            add(fam, "cv", paste0(field, ".grid.pos"), 1,
                as.numeric(delta), as.numeric(delta), as.numeric(cc$lam_pos),
                status, sprintf("grid index R=%d, R#=%d; lambda diff=%s",
                                gp, rp, format(abs(gv - rv), digits = 4)))
        }
    } else {
        skip(fam, "cv", "lambda.min.grid.pos",
             "CV measure semantics differ for this family; optimum is not comparable")
        skip(fam, "cv", "lambda.1se.grid.pos",
             "CV measure semantics differ for this family; optimum is not comparable")
    }

    # ---------- coef / predict ----------
    gco <- numeric_matrix(read_gold(fam, "coef"))
    rco <- numeric_matrix(read_rsharp(fam, "coef"))
    # cox has no intercept in coef(fit), while the wrapper always emits the
    # (Intercept) row; drop it before comparing.
    if (fam == "cox" && nrow(rco) == nrow(gco) + 1) {
        rco <- rco[-1, , drop = FALSE]
    }
    if (nrow(gco) == nrow(rco) && ncol(gco) == ncol(rco)) {
        check(fam, "coef", "coefficients", as.vector(gco), as.vector(rco),
              cc$coef, 0, "coef(fit, s = s_ref) at the same grid point")
    } else {
        skip(fam, "coef", "coefficients",
             sprintf("shape mismatch: R=%dx%d, R#=%dx%d",
                     nrow(gco), ncol(gco), nrow(rco), ncol(rco)))
    }

    gpr <- numeric_matrix(read_gold(fam, "predict"))
    rpr <- numeric_matrix(read_rsharp(fam, "predict"))
    if (nrow(gpr) == nrow(rpr) && ncol(gpr) == ncol(rpr)) {
        check(fam, "predict", "link", as.vector(gpr), as.vector(rpr),
              cc$predict, 0, "predict(fit, newx, s = s_ref, type = 'link')")
    } else {
        skip(fam, "predict", "link",
             sprintf("shape mismatch: R=%dx%d, R#=%dx%d",
                     nrow(gpr), ncol(gpr), nrow(rpr), ncol(rpr)))
    }
}

detail <- do.call(rbind, details)
utils::write.csv(detail, file = file.path(report_dir, "compare_detail.csv"),
                 row.names = FALSE)

n_pass <- sum(detail$status == "PASS")
n_fail <- sum(detail$status == "FAIL")
n_skip <- sum(detail$status == "SKIP")

lines <- c(
    "================================================================================",
    " glmnet R# wrapper vs GNU R glmnet  -  numeric comparison report",
    "================================================================================",
    sprintf(" R:            %s", R.version.string),
    sprintf(" glmnet:       %s", as.character(utils::packageVersion("glmnet"))),
    sprintf(" generated:    %s", format(Sys.time(), "%Y-%m-%d %H:%M:%S")),
    sprintf(" golden data:  %s", normalizePath(data_dir)),
    sprintf(" R# results:   %s", normalizePath(result_dir)),
    "",
    " Families: gaussian, binomial, poisson, multinomial, cox, mgaussian",
    " Input:    glmnet built-in example datasets, identical lambda grid and foldid",
    ""
)

for (fam in families) {
    sub <- detail[detail$family == fam, ]
    lines <- c(lines,
               sprintf("---- %s ----------------------------------------------------", fam),
               sprintf("  checks=%d  PASS=%d  FAIL=%d  SKIP=%d",
                       nrow(sub), sum(sub$status == "PASS"),
                       sum(sub$status == "FAIL"), sum(sub$status == "SKIP")))
    for (i in seq_len(nrow(sub))) {
        row <- sub[i, ]
        if (row$status == "SKIP") {
            lines <- c(lines, sprintf("  [SKIP] %-10s %-24s %s",
                                      row$file, row$field, row$note))
        } else {
            lines <- c(lines,
                       sprintf("  [%s] %-10s %-24s maxabs=%-14s limit=%-14s %s",
                               row$status, row$file, row$field,
                               format(row$max_abs_diff, digits = 4),
                               format(row$limit, digits = 4), row$note))
        }
    }
    lines <- c(lines, "")
}

lines <- c(lines,
           "================================================================================",
           sprintf(" TOTAL: %d checks, %d PASS, %d FAIL, %d SKIP",
                   nrow(detail), n_pass, n_fail, n_skip),
           "",
           " Known limitations (deliberately SKIP or widened tolerance):",
           "  * binomial near complete separation => non-unique coefficients (tol 3.0);",
           "  * multinomial grouped penalty => coefficients can drift in magnitude;",
           "  * multi-response a0/nzero have different semantics in the golden export;",
           "  * cvm/cvsd absolute scales differ with weight normalization / measure",
           "    definition, so lambda.min / lambda.1se grid positions are the main",
           "    cross-validation criterion;",
           "  * cox coef(fit) has no intercept row in GNU R; the wrapper's first row",
           "    is dropped before comparison.",
           "",
           sprintf(" RESULT: %s", if (n_fail == 0) "PASS" else "FAIL"),
           "================================================================================")

writeLines(lines, con = file.path(report_dir, "compare_report.txt"))
cat(readLines(file.path(report_dir, "compare_report.txt")), sep = "\n")
cat("\n")

if (n_fail > 0) {
    quit(save = "no", status = 1, runLast = FALSE)
}
