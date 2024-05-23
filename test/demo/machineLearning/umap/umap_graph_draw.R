imports ["dataset", "umap"] from "MLkit";

require(igraph);
require(visualizer);

setwd(@dir);

let umap = readBin("./umap_mnist.dat", what = "umap");
# cluster colors for rendering the nodes
let clusters = alpha([
	"#006400","#00008b","#b03060",
	"#ff4500","#ffd700","#7fff00",
	"#00ffff","#ff00ff","#6495ed",
	"#ffdab9"
], alpha = 0.9);

print("cluster colors:");
print(clusters);

bitmap(file = `./MNIST-LabelledVectorArray-20000x100.umap_graph.png`) {
    # data visualization
    umap 
    |> as.graph()
    # :> compute.network

    # set node cluster colors
    |> setColors(type = 0, color = clusters[1])
    |> setColors(type = 1, color = clusters[2])
    |> setColors(type = 2, color = clusters[3])
    |> setColors(type = 3, color = clusters[4])
    |> setColors(type = 4, color = clusters[5])
    |> setColors(type = 5, color = clusters[6])
    |> setColors(type = 6, color = clusters[7])
    |> setColors(type = 7, color = clusters[8])
    |> setColors(type = 8, color = clusters[9])
    |> setColors(type = 9, color = clusters[10])

    # run network graph rendering
    |> render(
        canvasSize        = [3840, 2160],
        padding           = "padding:200px 200px 200px 200px;",
        labelerIterations = -1,
        minNodeSize       = 30,
        minLinkWidth      = 0.5,
        nodeStroke        = "stroke: white; stroke-width: 1px; stroke-dash: dash;",
        showLabel         = FALSE,
        defaultEdgeColor  = "#F6F6F6",
        defaultEdgeDash   = "Dash"
    );
}

