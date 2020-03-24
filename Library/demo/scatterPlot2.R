imports "plot.charts" from "R.plot";

let data <- read.csv("K:\20200226\20200321\PCA3.csv");

print(head(data));

let names <- data[, "EigenValue"];
let keys <- unique(names);
let X <- as.numeric(data[, "X"]);
let Y <- as.numeric(data[, "Y"]);

data <- [];

for(name in keys) {
	let i <- name == names;
	let subset <- serial(X[i], Y[i], name = name, color = 'red', ptSize = 8);
	
	print(subset);
	
	data <- data << subset;
}

plot(data) :> save.graphics(file = "K:\20200226\20200321\PCA2D.png");
