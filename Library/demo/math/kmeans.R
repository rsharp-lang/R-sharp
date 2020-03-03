imports "stats.clustering" from "R.math.dll";

require(dataframe);

setwd(!script$dir);

["bezdekIris.csv"]
:> read.dataframe(mode = "numeric")
:> kmeans(centers = 4, parallel = TRUE)
:> summary
:> str
;