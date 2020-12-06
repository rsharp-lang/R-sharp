imports ["dataset", "umap"] from "MLkit";

require(igraph.render);

const MNIST_LabelledVectorArray as string = "E:\GCModeller\src\runtime\sciBASIC#\Data_science\DataMining\data\umap\MNIST-LabelledVectorArray-60000x100.msgpack";
const graph_visual as string = `${!script$dir}/MNIST-LabelledVectorArray-20000x100.umap_graph.png`;

let manifold = MNIST_LabelledVectorArray 
:> read.mnist.labelledvector(takes = 200)
:> umap(dimension = 2)
;

# data visualization
manifold$umap 
:> as.graph(labels = manifold$labels)
:> render.Plot(
	canvasSize = [4096, 3112],
	padding = "padding:100px 100px 100px 100px;",
	labelerIterations = -1	
)
:> save.graphics(file = graph_visual)
;