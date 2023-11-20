imports "clustering" from "MLkit";

require(charting);

setwd(@dir);

let rawdata = read.csv("./feature_regions.csv", row.names = 1, check.names = FALSE);

rawdata[, "Cluster"] = NULL;
rawdata[, "x"] = as.numeric(rawdata$x);
rawdata[, "y"] = as.numeric(rawdata$y);

print(rawdata, max.print = 13);

let result = kmeans(rawdata, centers = 9, bisecting = FALSE);
let result2 = kmeans(rawdata, centers = 9, bisecting = TRUE);

write.csv(result, file = "./kmeans.csv");
write.csv(result2, file = "./bisecting_kmeans.csv");

result = as.data.frame(result);
result2 = as.data.frame(result2);

print(result, max.print = 6);

bitmap(file = "./kmeans1.png") {
	plot(as.numeric(result[, "x"]), as.numeric(result[, "y"]), 
		class     = result$Cluster, 
		grid.fill = "white",
		padding   = "padding: 125px 300px 200px 200px;",
		colorSet  = "paper"
	);
}

bitmap(file = "./kmeans2.png") {
	plot(as.numeric(result2[, "x"]), as.numeric(result2[, "y"]), 
		class     = result2$Cluster, 
		grid.fill = "white",
		padding   = "padding: 125px 300px 200px 200px;",
		colorSet  = "paper"
	);
}