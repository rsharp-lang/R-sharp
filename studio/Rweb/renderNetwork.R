imports ["igraph", "igraph.layouts", "igraph.render"] from "R.graph";

let edge = (?"edge.csv" || stop("no network data provided!")) :> read.csv;
let node = (?"node.csv" || stop("no node data provided!")) :> read.csv;
let file_save = (output_device() == "html") ? NULL : "./render.svg";

edge[, "correlation"] <- as.numeric(edge[, "correlation"]);
edge[, "fdr"] <- as.numeric(edge[, "fdr"]);

# print(head(node));
# print(head(edge));

let guid = edge[, 1];
let a = edge[, "compoundA"];
let b = edge[, "compoundB"];
let w = abs(edge[, "correlation"]) / 4;
let node_size = as.numeric(node[, "log2FC"]);
let p = -log( node[, "p"], 10);
let nodes = as.character(node[, "names"]);
let colorLsit = colors("RdYlGn:c8", 100, character = TRUE); 

p = 1 + as.integer(49 * p / max(p));

colorLsit = lapply(1:length(nodes), i => (node_size[i] > 0.0) ? colorLsit[50 + p[i]] : colorLsit[50 - p[i]], names = i => nodes[i]);

node_size = abs(node_size) * 20;
node_size = lapply(1:length(nodes), i => node_size[i], names = i => nodes[i]);
edge = lapply(1:length(guid), i => [a[i], b[i]], names = i => guid[i]);

# str(edge);

let g = empty.network()
:> add.nodes(nodes)
:> add.edges(edge, weight = w)
:> compute.network()
# :> layout.force_directed(showProgress = FALSE)
:> layout.orthogonal(gridSize = [1000,1000])
:> node.colors(colorLsit)
:> render.Plot(canvasSize = [1600,1200], nodeSize = node_size, defaultNodeSize = 20, driver = "SVG", labelerIterations= 0)
:> save.graphics(file = file_save)
;