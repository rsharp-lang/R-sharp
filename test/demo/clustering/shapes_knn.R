imports "clustering" from "MLkit";

let multishapes = read.csv(system.file("data/multishapes.csv", package = "REnv"), row.names = NULL, check.names = FALSE);

print(multishapes, max.print = 16);

multishapes[,"shape"] = NULL;

multishapes = knn_cluster(multishapes, knn = 32, p = 0.8);
multishapes = as.data.frame(multishapes);

print(multishapes, max.print = 16);

# show object detection result
bitmap(file = relative_work( "shapes_knn.png")) {
	plot(as.numeric(multishapes[, "x"]), as.numeric(multishapes[, "y"]), 
		class     = multishapes[, "Cluster"], 
		grid.fill = "white",
		padding   = "padding: 125px 300px 200px 200px;",
		colorSet  = "paper"
	);
}

