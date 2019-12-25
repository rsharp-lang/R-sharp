require(igraph);
require(igraph.layouts);

let test.network as string = `${!script$dir}/demo_network`;
let g = read.network(test.network);

# view summary info of the network
print(g);

g
:> layout.orthogonal
:> save.network(file = test.network);