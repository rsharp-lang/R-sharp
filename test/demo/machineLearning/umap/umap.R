imports ["dataset", "umap"] from "MLkit";

require(igraph);
require(visualizer);

setwd(@dir);

const MNIST_LabelledVectorArray as string = "\GCModeller\src\runtime\sciBASIC#\Data_science\DataMining\data\umap\MNIST-LabelledVectorArray-60000x100.msgpack";

let manifold = MNIST_LabelledVectorArray 
|> read.MNIST(subset = 2000, dataset = "dataframe")
:> umap(
	dimension         = 2, 
	numberOfNeighbors = 15,
    localConnectivity = 1,
    KnnIter           = 32,
    bandwidth         = 1
)
;

writeBin(manifold$umap, con = "./umap_mnist.dat", labels = manifold$labels);
