require(igraph);
require(igraph.layouts);
require(igraph.render);

# create a new empty network graph model
let g = empty.network();

# and then add nodes by given id list
for(id in ["A","B","C","D","E", "F", "G", "H"]) {
	g :> add.node(label = id);
}

for(id in ["A1","B1","C1","D1","E1", "F1", "G1", "H1"]) {
	g :> add.node(label = id);
}

for(id in ["X","Y","Z"]) {
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

g :> add.edge("B1", "H");
g :> add.edge("B1", "A1");
g :> add.edge("B1", "C1");
g :> add.edge("B1", "D1");
g :> add.edge("D1", "E1");
g :> add.edge("E1", "F1");
g :> add.edge("F1", "G1");
g :> add.edge("G1", "H1");

g :> add.edge("H", "H1");

g :> add.edge("X", "Y");
g :> add.edge("Y", "Z");
g :> add.edge("X", "Z");

g :> add.edge("X", "A");
g :> add.edge("X", "A1");

# do network styling
g 
:> type_groups(type = "A+",      nodes = ["A","B","C"]);
:> type_groups(type = "B_class", nodes = ["D"]);
:> type_groups(type = "tails",   nodes = ["F", "G"]);
:> type_groups(type = "mirror",  nodes = ["A1","B1","C1","D1","E1", "F1", "G1", "H1"]);

g 
:> color.type_group(type = "A+",      color = "red");
:> color.type_group(type = "B_class", color = "green");
:> color.type_group(type = "tails",   color = "yellow");
:> color.type_group(type = "mirror",  color = "purple");

print(g :> degree);

# Then we can do network layout and 
# save the generated network model in csv file tables
g 
:> layout.force_directed 
:> save.network(file = `${!script$dir}/demo_network`);

# Do network render plot and then 
# saved the generated image file
g 
:> render.Plot(canvasSize = [600,400]) 
:> save.graphics(file = `${!script$dir}/plot.png`);