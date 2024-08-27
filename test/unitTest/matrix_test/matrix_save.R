require(Matrix);

let rawdata = read.csv("\GCModeller\src\R-sharp\REnv\data\wine.csv", row.names = 1, check.names = FALSE);

setwd(@dir);

print(rawdata);

let m = as.matrix(rawdata);
# let [W,H] = nmf(m, rank = 2);

# W = as.data.frame(W);
# W[,"x1"] = as.integer(W$x1);
# W[,"x2"] = as.integer(W$x2);

# print(W);

print(sum(as.data.frame(m)[,1]));

writeBin(m, con = "./wine.dat");

m=NULL;
m = readBin("./wine.dat", what = "LA_mat");

# W=NULL;
# H=NULL;

# [W,H] = nmf(m, rank = 2);

# W = as.data.frame(W);
# W[,"x1"] = as.integer(W$x1);
# W[,"x2"] = as.integer(W$x2);

# print(W);
print(sum(as.data.frame(m)[,1]));