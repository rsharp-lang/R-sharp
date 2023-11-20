imports "clustering" from "MLkit";

setwd(@dir);

let rawdata = read.csv("./feature_regions.csv", row.names = 1, check.names = FALSE);

rawdata[, "Cluster"] = NULL;
rawdata[, "x"] = as.numeric(rawdata$x);
rawdata[, "y"] = as.numeric(rawdata$y);

print(rawdata, max.print = 13);

let result = kmeans(rawdata, centers = 6, bisecting = FALSE);
let result2 = kmeans(rawdata, centers = 6, bisecting = TRUE);

write.csv(result, file = "./kmeans.csv");
write.csv(result2, file = "./bisecting_kmeans.csv");
