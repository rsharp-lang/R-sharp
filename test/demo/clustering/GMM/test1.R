imports "clustering" from "MLkit";

setwd(@dir);

# https://github.com/thearrow/ai-GMM/tree/master

let x = as.numeric(readLines("./data1.txt"));
let gmm = gmm(x, 3);

