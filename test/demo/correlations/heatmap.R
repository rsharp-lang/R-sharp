imports "charts" from "R.plot";

setwd(!script$dir);

# let raw = read.csv("correlation.csv");
let raw = read.csv("DAM_correlation.csv");

print(head(raw));

let matrix = raw :> as.dist(
	is_matrix = FALSE, 
	f1  = "compoundA", 
	f2  = "compoundB", 
	val = "correlation"
);

print(matrix);

matrix 
:> plot(size="3300,2700", padding = "padding: 150px 250px 200px 100px;", colors ="Spectral:c8", fixed_size = FALSE) 
:> save.graphics(file = "cor_DAM.png");