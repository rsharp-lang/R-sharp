imports ["igraph", "igraph.layouts", "igraph.render"] from "igraph";

#' title: Render Network
#' author: xieguigang <xie.guigang@gcmodeller.org>
#' description: Rendering network as png image.

const edges as string   = ?"--cor"       || stop("no network data provided!");
const nodes as string   = ?"--DAM"       || stop("no node data provided!");
const outputdir         = ?"--outputdir" || `${dirname(edges)}/renderNetwork/`;
const cor_cut as double = ?"--cor.cut"   || 0.6;

const processingNetwork as function(edge, node) {
	edge[, "correlation"] <- as.numeric(edge[, "correlation"]);
	edge[, "pValue"]      <- as.numeric(edge[, "pValue"]);

	# reduce the size of the network
	edge = edge[abs(edge[, "correlation"]) >= cor_cut, ];

	let guid   = 1:nrow(edge);
	let a      = edge[, "compoundA"];
	let b      = edge[, "compoundB"];
	let w      = abs(edge[, "correlation"]) / 2;
	let log2FC = colnames(node)[colnames(node) like "log2(FC_*)"];
	let node_size as double;

	if (length(log2FC) > 0) {
		node_size = as.numeric(node[, log2FC]);
	} else {
		if ("VIP" in colnames(node)) {
			node_size = as.numeric(node[, "VIP"]);
		} else {
			stop("no node size data provided, please ensure that DAM table contains VIP or log2FC column...");
		}
	}

	# config node style: size and colors
	let p         = -log( node[, "p.value"], 10);
	let nodes     = as.character(node[, 1]);
	let colorLsit = colors("RdYlGn:c8", 100, character = TRUE); 

	p = 1 + as.integer(49 * p / max(p));

	colorLsit = lapply(1:length(nodes), i => (node_size[i] > 0.0) ? colorLsit[50 + p[i]] : colorLsit[51 - p[i]], names = i -> nodes[i]);

	node_size = abs(node_size) * 20;
	node_size = lapply(1:length(nodes), function(i) {
		if (node_size[i] > 150) {
			150;
		} else {
			node_size[i];
		}
	}, names = i -> nodes[i]);

	edge = lapply(1:length(guid), i => [a[i], b[i]], names = i -> guid[i]);

	# str(node_size);
	# str(edge);

	empty.network()
	:> add.nodes(nodes)
	:> add.edges(edge, weight = w)
	:> connected_graph
	:> compute.network
	:> layout.force_directed(
		showProgress = FALSE, 
		stiffness    = 85,
		repulsion    = 4000,
		damping      = 0.83,
		iterations   = 2000
	)
	:> node.colors(colorLsit)
	;
}

bitmap(file = `${outputdir}/renderNetwork.png`) {
	# do render
	const g = processingNetwork(
		edge = read.csv(edges), 
		node = read.csv(nodes)
	);
	
	save.network(g, file = `${dirname(out)}/cor_network/`);
	plot(g,
		canvasSize        = [1800,1600], 
		padding           = 50, 
		nodeSize          = node_size, 
		defaultNodeSize   = 30, 
		labelerIterations = 0
	);
}