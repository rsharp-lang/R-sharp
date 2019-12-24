require(igraph);

let g = empty.network() :> as.object;

for(id in ["A","B","C","D","E"]) {
	g$CreateNode(id);
}

g :> save.network(file = "./demo_network");

