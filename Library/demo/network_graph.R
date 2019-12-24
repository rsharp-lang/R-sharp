require(igraph);

let g = empty.network();

for(id in ["A","B","C","D","E"]) {
	g :> add.node(label = id);
}

g :> save.network(file = `${!script$dir}/demo_network`);

