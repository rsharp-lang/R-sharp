imports "stats.clustering" from "R.math";

setwd(!script$dir);

let raw = read.csv("multishapes.csv")[, ["x","y"]];

print("previews of the raw input data:");
print(head(raw));

let result = as.object(dbscan(raw, 1))$cluster;

str(summary(result));

as.data.frame(result) :> write.csv(file = "multishapes_dbscan.csv", row_names = FALSE);
