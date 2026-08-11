# Generate R 4.5.0 test rda/rds samples covering all base types + ALTREP scenarios
setwd("g:/GCModeller/src/R-sharp/studio/RData/test/data")

# --- basic atomic vectors ---
int_vec  <- 1:50
real_vec <- c(1.5, 2.5, 3.5, -0.25, 100.0, NaN, Inf, -Inf)
lgl_vec  <- c(TRUE, FALSE, NA, TRUE)
str_vec  <- c("hello", "world", "R#", "数据", NA, "utf8-测试")
cplx_vec <- c(1+2i, 3-4i, 0+0i, NA_complex_)
raw_vec  <- as.raw(c(0x00, 0x01, 0x7f, 0xff, 0xab))

# --- named vector ---
named_vec <- c(a = 1, b = 2, c = 3, d = 4)

# --- ALTREP scenarios ---
altrep_intseq   <- 1:100000          # compact_intseq
altrep_realseq <- seq(0, 1, by = 0.01)  # compact_realseq (double)
altrep_seq_len  <- seq_len(50000)    # compact_intseq like
# deferred string: large repeated character vector triggers deferred_string ALTREP
deferred_str    <- rep(c("apple", "banana", "cherry"), times = 20000)

# --- factor ---
factor_vec <- factor(c("low", "mid", "high", "mid", "low"), levels = c("low", "mid", "high"))

# --- matrix ---
mat <- matrix(1:12, nrow = 3, ncol = 4, dimnames = list(c("r1","r2","r3"), c("c1","c2","c3","c4")))

# --- list (flat) ---
lst <- list(a = 1:5, b = c("x", "y", "z"), c = matrix(1:4, 2, 2), d = factor_vec)

# --- nested list ---
nested <- list(
  level1 = list(
    level2 = list(
      x = 1:3,
      y = c("p", "q"),
      z = list(deep = c(TRUE, FALSE, NA))
    )
  ),
  vec = real_vec
)

# --- data.frame ---
df <- data.frame(
  id   = 1:5,
  name = c("a", "b", "c", "d", "e"),
  val  = c(1.1, 2.2, 3.3, 4.4, 5.5),
  flag = c(TRUE, FALSE, TRUE, FALSE, TRUE),
  stringsAsFactors = FALSE
)
df_with_factor <- data.frame(
  grp = factor(c("A", "B", "A", "C")),
  score = c(10, 20, 30, 40),
  stringsAsFactors = TRUE
)

# --- object with attributes (ts) ---
ts_obj <- ts(1:20, start = c(2020, 1), frequency = 12)

# --- save as rda ---
save(int_vec, real_vec, lgl_vec, str_vec, cplx_vec, raw_vec, named_vec,
     altrep_intseq, altrep_realseq, altrep_seq_len, deferred_str,
     factor_vec, mat, lst, nested, df, df_with_factor, ts_obj,
     file = "samples.rda")

# --- save individual objects as rds ---
saveRDS(int_vec, "int_vec.rds")
saveRDS(str_vec, "str_vec.rds")
saveRDS(altrep_intseq, "altrep_intseq.rds")
saveRDS(altrep_realseq, "altrep_realseq.rds")
saveRDS(deferred_str, "deferred_str.rds")
saveRDS(nested, "nested.rds")
saveRDS(df, "df.rds")
saveRDS(df_with_factor, "df_with_factor.rds")
saveRDS(factor_vec, "factor_vec.rds")
saveRDS(mat, "mat.rds")
saveRDS(cplx_vec, "cplx_vec.rds")
saveRDS(raw_vec, "raw_vec.rds")
saveRDS(ts_obj, "ts_obj.rds")
saveRDS(named_vec, "named_vec.rds")

# --- reference / shared object (same object referenced twice) ---
shared <- 1:100
ref_list <- list(first = shared, second = shared)
saveRDS(ref_list, "ref_list.rds")

cat("DONE\n")
