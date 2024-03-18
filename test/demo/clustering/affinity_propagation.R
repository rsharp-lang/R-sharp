imports "clustering" from "MLkit";

let rawdata = read.csv(file.path(@dir, "feature_regions.csv"), row.names = 1, check.names = FALSE);

rawdata[, "Cluster"] = NULL;
rawdata[, "x"] = as.numeric(rawdata$x);
rawdata[, "y"] = as.numeric(rawdata$y);

print(rawdata, max.print = 13);

let result = as.data.frame(affinity_propagation(rawdata));

write.csv(result, file = file.path(@dir, "affinity_propagation.csv"));

bitmap(file = file.path(@dir, "affinity_propagation.png")) {
	plot(as.numeric(result[, "x"]), as.numeric(result[, "y"]), 
		class     = result$Cluster, 
		grid.fill = "white",
		padding   = "padding: 125px 300px 200px 200px;",
		colorSet  = "paper"
	);
}