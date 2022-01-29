setwd(@dir);

# read scatter point data from a given table file
# and then assign to tuple variables
umap = read.csv("./MNIST-LabelledVectorArray-20000x100.umap_scatter3.csv", row.names = 1);

# umap scatter with class colors
bitmap(file = "./scatter.png") {
	plot(umap[, "dimension_1"], umap[, "dimension_2"],
		 padding      = "padding:200px 400px 200px 250px;",
		 class        = rownames(umap),
		 title        = "UMAP of MNIST numbers",
		 x.lab        = "dimension 1",
		 y.lab        = "dimension 2",
		 legend.block = 6,
		 colorSet     = "paper", 
		 grid.fill    = "transparent",
		 size         = [2600, 1600]
	);
};