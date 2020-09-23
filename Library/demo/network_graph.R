require(igraph);
require(igraph.layouts);
require(igraph.render);

# create a new empty network graph model
let g = empty.network();

# and then add nodes by given id list
g 
:> add.nodes(labels = ["A","B","C","D","E", "F", "G", "H"]) 
:> add.nodes(labels = ["A1","B1","C1","D1","E1", "F1", "G1", "H1"]);

# add node one by one
for(id in ["X","Y","Z"]) {
	g :> add.node(label = id, size = 9);
}

# add edges between the specific nodes tuples
g 
:> add.edges(list(["A", "B"],  ["B", "C"],  ["C", "D"],  ["C", "E"],  ["A", "E"],   ["A", "F"],   ["F", "G"],   ["F", "H"],  ["B", "H"]))
:> add.edges(list(["B1", "H"], ["B1", "A1"], ["B1", "C1"], ["B1", "D1"], ["D1", "E1"], ["E1", "F1"], ["F1", "G1"], ["G1", "H1"]));

# add edges between nodes one by one
g :> add.edge("H", "H1");

g :> add.edge("X", "Y");
g :> add.edge("Y", "Z");
g :> add.edge("X", "Z");

g :> add.edge("X", "A");
g :> add.edge("X", "A1");

# do network styling
g 
:> set_group(type = "A+",      nodes = ["A","B","C"])
:> set_group(type = "B_class", nodes = ["D"])
:> set_group(type = "tails",   nodes = ["F", "G"])
:> set_group(type = "mirror",  nodes = ["A1","B1","C1","D1","E1", "F1", "G1", "H1"]);

g 
:> color.group(type = "A+",      color = "red")
:> color.group(type = "B_class", color = "green")
:> color.group(type = "tails",   color = "yellow")
:> color.group(type = "mirror",  color = "purple");

cat("\n");
str(g :> degree);

# display console progress bar on current y location
console::progressbar.pin.top();

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