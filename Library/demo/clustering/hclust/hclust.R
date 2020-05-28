imports "stats.clustering" from "R.math";
imports "charts" from "R.plot";

setwd(!script$dir);

read.csv("data.csv")
:> as.dist(type = "dist")
:> hclust
:> plot
:> save.graphics("./hclust.png")