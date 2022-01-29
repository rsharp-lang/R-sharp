imports "clustering" from "MLkit";

require(graphics2D);

setwd(@dir);

multishapes = read.csv("./multishapes.csv");

print(multishapes, max.print = 13);

# detect object from umap data
objects = graphics2D::pointVector(multishapes[, "x"], multishapes[, "y"]) |> dbscan_objects();

# show object detection result
bitmap(file = "./object_detection.png") {
	plot(multishapes[, "x"], multishapes[, "y"], 
		class     = `object_${objects}`, 
		grid.fill = "white",
		padding = "padding: 125px 200px 200px 200px;"
	);
}
