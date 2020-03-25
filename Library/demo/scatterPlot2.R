imports "plot.charts" from "R.plot";

let data <- read.csv("K:\20200226\20200321\PCA3.csv");

print(head(data));

let names <- data[, "EigenValue"];
let keys <- unique(names);
let X <- as.numeric(data[, "X"]);
let Y <- as.numeric(data[, "Y"]);
let colorList <- colors("d3.scale.category20b()", length(keys));

data <- [];

for(j in 1:length(keys)) {
	let name <- keys[j];
	let i <- name == names;
	let subset <- serial(X[i], Y[i], name = name, color = colorList[j], ptSize = 50, alpha = 150);
	
	print(subset);
	
	data <- data << subset;
}

X <- line2D([-0.75, 0], [2.5, 0]);
Y <- line2D([0, 0.4], [0, -1.6]);

plot(data, 
	size = [3200, 2700], 
	padding = [250,150,250,250], 
	title = "Principal Coordinates",
	x.lab = "PC1 (93.124%)",
	y.lab = "PC2 (82.057%)",
	legend.font = "font-style: normal; font-size: 20; font-family: Bookman Old Style",
	legend.block = 7,# abline = [X, Y]
	convexHull = ["China"]
) :> save.graphics(file = "K:\20200226\20200321\PCA2D.png");
