imports "charts" from "R.plot";

setwd(!script$dir);

let raw = read.csv("correlation.csv");

print(head(raw));

let matrix = raw :> as.dist("compoundA", "compoundB", "correlation");

print(matrix);

matrix :> plot :> save.graphics(file = "cor.png");