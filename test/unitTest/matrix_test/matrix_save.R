require(Matrix);

let rawdata = read.csv("\GCModeller\src\R-sharp\REnv\data\wine.csv", row.names = 1, check.names = FALSE);

setwd(@dir);

print(rawdata);

let m = as.matrix(rawdata);

print(m);

writeBin(m, con = "./wine.dat");

m = readBin("./wine.dat", what = "LA_mat");

print(m);