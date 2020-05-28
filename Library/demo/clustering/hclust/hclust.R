imports "stats.clustering" from "R.math";
imports "charts" from "R.plot";

setwd(!script$dir);

read.csv("data.csv", row_names = 1)
:> as.dist(type = "dist")
:> hclust
:> plot
:> save.graphics("./hclust.png")