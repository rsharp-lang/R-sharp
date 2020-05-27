imports "charts" from "R.plot";

setwd(!script$dir);

let raw = read.csv("correlation.csv");

print(head(raw));

let matrix = raw :> as.dist("compoundA", "compoundB", "correlation");

print(matrix);

matrix 
:> plot(size="3300,2700", padding = "padding: 150px 250px 200px 100px;", colors ="Spectral:c8", fixed_size = FALSE) 
:> save.graphics(file = "cor.png");