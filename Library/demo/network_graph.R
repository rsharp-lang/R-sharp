require(igraph);
require(igraph.layouts);
require(igraph.render);

let g = empty.network();

for(id in ["A","B","C","D","E"]) {
	g :> add.node(label = id);
}

g :> add.edge("A", "B");
g :> add.edge("B", "C");
g :> add.edge("C", "D");
g :> add.edge("C", "E");
g :> add.edge("A", "E");

g 
:> layout.force_directed 
:> save.network(file = `${!script$dir}/demo_network`);

g 
:> render.Plot 
:> save.graphics(file = `${!script$dir}/plot.png`);