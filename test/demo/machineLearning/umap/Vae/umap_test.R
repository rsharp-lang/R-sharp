imports ["dataset", "umap"] from "MLkit";

require(igraph);
require(visualizer);

setwd(@dir);

options(n_threads = 24);

let umap_process = function(x, export_dir) {
	let embedding = umap(x, dimension         = 3,numberOfNeighbors = 32);
	embedding = as.data.frame(embedding, labels = rownames(x),
		dimension = ["x", "y", "z"]);

	embedding[, "class"] = $"N_\d+"(rownames(x));

	print(embedding, max.print = 6);
	write.csv(embedding, file = file.path(export_dir, "umap.csv"));

	bitmap(file = `${export_dir}/plot_mnist.png`, size = [6000,4000]) {
		plot(embedding$x, embedding$y,
			class       = embedding$class, 
			labels      = rownames(embedding),
			show_bubble = FALSE,
			point_size  = 25,
			legendlabel = "font-style: normal; font-size: 24; font-family: Bookman Old Style;",
			padding     = "padding:150px 150px 350px 350px;",
			background  = "white",
			colors      = "paper"
		);
	}
}

umap_process(read.csv("old/nmf_class.csv", row.names = 1, check.names = FALSE), export_dir = "./test_sequence");
umap_process(read.csv("nmf_class.csv", row.names = 1, check.names = FALSE), export_dir = "./test_parallel");