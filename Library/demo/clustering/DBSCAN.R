imports "stats.clustering" from "R.math";

setwd(!script$dir);

let raw = read.csv("multishapes.csv")[, ["x","y"]];

print("previews of the raw input data:");
print(head(raw));

let result = as.object(dbscan(raw, 0.01))$cluster;

summary(result);

as.data.frame(result) :> write.csv(file = "multishapes_dbscan.csv");
