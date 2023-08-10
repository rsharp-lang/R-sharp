imports "clustering" from "MLkit";

setwd(@dir);

let x = read.csv("../feature_regions.csv", row.names = 1, check.names = FALSE);

x[,"Cluster"] = NULL;

print(x);

let gmm = gmm(x, 5);

print(gmm.predict_proba(gmm));

write.csv(gmm.predict_proba(gmm), file = "./cluster2.csv", row.names = TRUE);