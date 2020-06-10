require(igraph);
require(igraph.layouts);
require(igraph.render);

# let test.network as string = `${!script$dir}/demo_network`;
let test.network = "D:\biodeep\biodeepdb_v3\Rscript\metacluster\old_network\Flavonoids\tree_network";
let g = read.network(test.network);

# create a new empty network graph model
# let g = empty.network()
# :> add.nodes(labels = ["A","B","C","D"]);

# g :> add.edge("A", "B");
# g :> add.edge("A", "C");
# g :> add.edge("C", "D");
# g :> add.edge("D", "B");

# view summary info of the network
print(g);

g
:> layout.orthogonal(layoutIteration = 10)
# Do network render plot and then 
# saved the generated image file
:> render.Plot(canvasSize = [2000,1400]) 
:> save.graphics(file = `${!script$dir}/orthogonal.png`);