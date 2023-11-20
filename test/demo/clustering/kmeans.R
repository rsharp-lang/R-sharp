imports "clustering" from "MLkit";

setwd(@dir);

let rawdata = read.csv("./feature_regions.csv", row.names = 1, check.names = FALSE);

print(rawdata, max.print = 13);

