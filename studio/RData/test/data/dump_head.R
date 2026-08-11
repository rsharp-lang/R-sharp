dd <- "g:/GCModeller/src/R-sharp/studio/RData/test/data/"
b <- readBin(paste0(dd, "altrep_realseq.rds"), "raw", n = 1e6)
b <- memDecompress(b, type = "gzip")
cat("len:", length(b), "\n")
cat(paste(as.integer(b[1:44]), collapse = " "), "\n")
