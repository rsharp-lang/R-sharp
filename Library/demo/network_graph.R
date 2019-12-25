require(igraph);
require(igraph.layouts);
require(igraph.render);

# create a new empty network graph model
let g = empty.network();

# and then add nodes by given id list
for(id in ["A","B","C","D","E"]) {
	g :> add.node(label = id);
}

# add edges between the specific nodes tuples
g :> add.edge("A", "B");
g :> add.edge("B", "C");
g :> add.edge("C", "D");
g :> add.edge("C", "E");
g :> add.edge("A", "E");

# Then we can do network layout and 
# save the generated network model in csv file tables
g 
:> layout.force_directed 
:> save.network(file = `${!script$dir}/demo_network`);

# Do network render plot and then 
# saved the generated image file
g 
:> render.Plot 
:> save.graphics(file = `${!script$dir}/plot.png`);