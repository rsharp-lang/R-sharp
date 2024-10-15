imports ["dataset", "umap"] from "MLkit";

require(igraph);
require(visualizer);

let export_dir = file.path(@dir, "test_sequence");
let embedding = read.csv(file.path(export_dir,"umap.csv"), 
    row.names = 1, 
    check.names = FALSE);

bitmap(file = `${export_dir}/plot_mnist.png`, size = [6000,4000], res = 300) {
    plot(embedding$x, embedding$y,
        class       = embedding$class, 
        labels      = rownames(embedding),
        show_bubble = FALSE,
        point_size  = 25,
        legendlabel = "font-style: normal; font-size: 24; font-family: Bookman Old Style;",
        padding     = "padding: 2% 5% 5% 5%;",
        fill  = "white",
        colors      = "paper"
    );
}