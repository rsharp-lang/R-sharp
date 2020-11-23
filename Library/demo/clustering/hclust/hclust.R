imports "stats.clustering" from "R.math";
imports "charts" from "R.plot";

setwd(!script$dir);

read.csv("data.csv", row_names = 1)
:> as.dist(type = "dist")
:> hclust
:> plot(class = read.list("sampleInfo.json"), padding = "padding: 200px 200px 200px 200px;")
:> save.graphics("./hclust.png")
;