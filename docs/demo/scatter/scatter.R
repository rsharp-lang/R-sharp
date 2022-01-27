setwd(@dir);

# read scatter point data from a given table file
# and then assign to tuple variables
[x, y, cluster] = read.csv("./scatter.csv", row.names = NULL);

# umap scatter with class colors
bitmap(file = "./scatter.png") {
	plot(x, y,
		 padding      = "padding:200px 400px 200px 250px;",
		 class        = cluster,
		 title        = "UMAP 2D",
		 x.lab        = "dimension 1",
		 y.lab        = "dimension 2",
		 legend.block = 13,
		 colorSet     = "paper", 
		 grid.fill    = "transparent",
		 size         = [2600, 1600]
	);
};