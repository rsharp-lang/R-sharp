imports ["dataset", "umap"] from "MLkit";

require(igraph);
require(visualizer);

setwd(@dir);

const MNIST_LabelledVectorArray as string = "./MNIST-LabelledVectorArray-60000x100.msgpack";

options(n_threads = 24);

let manifold = MNIST_LabelledVectorArray 
|> read.MNIST(subset = 60000, dataset = "dataframe")
|> umap(
	dimension         = 3, 
	numberOfNeighbors = 32,
    localConnectivity = 1,
    KnnIter           = 64,
    bandwidth         = 1
)
;

writeBin(manifold$umap, con = "./umap_mnist.dat", labels = manifold$labels);

manifold = as.data.frame(manifold$umap, 
    labels = manifold$labels, 
    dimension = ["x", "y", "z"]);

print(manifold, max.print = 13);

bitmap(file = `./plot_mnist.png`, size = [6000,4000]) {
	plot(manifold$x, manifold$y,
		class       = rownames(manifold), 
		labels      = rownames(manifold),
		show_bubble = FALSE,
		point_size  = 25,
		legendlabel = "font-style: normal; font-size: 24; font-family: Bookman Old Style;",
		padding     = "padding:150px 150px 350px 350px;",
        background  = "white",
        colors      = "paper"
	);
}