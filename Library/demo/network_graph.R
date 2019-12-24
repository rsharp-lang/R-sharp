require(igraph);

let g = empty.network();

for(id in ["A","B","C","D","E"]) {
	g :> add.node(label = id);
}

g :> add.edge("A", "B");
g :> add.edge("B", "C");
g :> add.edge("C", "D");
g :> add.edge("C", "E");
g :> add.edge("A", "E");

g :> save.network(file = `${!script$dir}/demo_network`);

