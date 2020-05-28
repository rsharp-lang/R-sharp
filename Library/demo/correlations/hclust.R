imports "stats.clustering" from "R.math";
imports "charts" from "R.plot";

setwd(!script$dir);

read.csv("correlation.csv")
:> as.dist("compoundA","compoundB","correlation")
:> hclust
:> plot
:> save.graphics("./hclust.png")