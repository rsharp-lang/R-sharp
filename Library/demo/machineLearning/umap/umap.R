imports ["dataset", "umap"] from "MLkit";

require(igraph);
require(igraph.render);

const MNIST_LabelledVectorArray as string = "E:\GCModeller\src\runtime\sciBASIC#\Data_science\DataMining\data\umap\MNIST-LabelledVectorArray-60000x100.msgpack";
const graph_visual as string = `${!script$dir}/MNIST-LabelledVectorArray-20000x100.umap_graph.png`;
const clusters as string = ["#006400","#00008b","#b03060","#ff4500","#ffd700","#7fff00","#00ffff","#ff00ff","#6495ed","#ffdab9"];

let manifold = MNIST_LabelledVectorArray 
:> read.mnist.labelledvector(takes = 1000)
:> umap(dimension = 2)
;

# data visualization
manifold$umap 
:> as.graph(labels = manifold$labels, groups = manifold$labels)
# :> compute.network

# set node cluster colors
:> setColors(type = 0, color = clusters[1])
:> setColors(type = 1, color = clusters[2])
:> setColors(type = 2, color = clusters[3])
:> setColors(type = 3, color = clusters[4])
:> setColors(type = 4, color = clusters[5])
:> setColors(type = 5, color = clusters[6])
:> setColors(type = 6, color = clusters[7])
:> setColors(type = 7, color = clusters[8])
:> setColors(type = 8, color = clusters[9])
:> setColors(type = 9, color = clusters[10])

# run network graph rendering
:> render.Plot(
	canvasSize        = [3840, 2160],
	padding           = "padding:100px 100px 100px 100px;",
	labelerIterations = -1,
	minNodeSize       = 20,
	nodeStroke        = "stroke: lightgray; stroke-width: 2px; stroke-dash: dash;",
	showLabel         = FALSE
)
:> save.graphics(file = graph_visual)
;