require(igraph);
require(igraph.layouts);
require(igraph.render);

# create a new empty network graph model
let g = empty.network();

# and then add nodes by given id list
for(id in ["A","B","C","D","E", "F", "G", "H"]) {
	g :> add.node(label = id);
}

# add edges between the specific nodes tuples
g :> add.edge("A", "B");
g :> add.edge("B", "C");
g :> add.edge("C", "D");
g :> add.edge("C", "E");
g :> add.edge("A", "E");
g :> add.edge("A", "F");
g :> add.edge("F", "G");
g :> add.edge("F", "H");
g :> add.edge("B", "H");

# Then we can do network layout and 
# save the generated network model in csv file tables
g 
:> layout.force_directed 
:> save.network(file = `${!script$dir}/demo_network`);

# do network styling
g :> type_groups(type = "A+",      nodes = ["A","B","C"]);
g :> type_groups(type = "B_class", nodes = ["D"]);
g :> type_groups(type = "tails",   nodes = ["F", "G"]);

g :> color.type_group(type = "A+",      color = "red");
g :> color.type_group(type = "B_class", color = "green");
g :> color.type_group(type = "tails",   color = "yellow");

# Do network render plot and then 
# saved the generated image file
g 
:> render.Plot(canvasSize = [600,400]) 
:> save.graphics(file = `${!script$dir}/plot.png`);