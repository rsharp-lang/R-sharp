setwd(@dir);

options(n_thread = 8);

let rawdata = read.csv("D:\GCModeller\src\R-sharp\REnv\data\wine.csv", row.names = 1, check.names = FALSE);
let pca = prcomp(rawdata, pc = 3);

print(pca);