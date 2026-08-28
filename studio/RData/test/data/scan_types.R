dd <- "g:/GCModeller/src/R-sharp/studio/RData/test/data/"
scan <- function(n) {
    b <- readBin(paste0(dd, n), "raw", n = 1e6)
    b <- memDecompress(b, type = "gzip")
    pos <- c()
    for (i in seq(1, length(b) - 3, by = 4)) {
        v <- readBin(as.raw(b[i:(i + 3)]), "int", 1, endian = "big")
        t <- bitwAnd(v, 255)
        if (t == 14 || t == 238 || t == 45) pos <- c(pos, i)
    }
    cat(n, "types@pos:", paste(pos[1:10], collapse = " "), "\n")
}
scan("int_vec.rds")
scan("altrep_realseq.rds")
scan("altrep_intseq.rds")
