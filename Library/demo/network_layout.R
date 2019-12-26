require(igraph);
require(igraph.layouts);
require(igraph.render);

let test.network as string = `${!script$dir}/demo_network`;
let g = read.network(test.network);

# view summary info of the network
print(g);

g
:> layout.orthogonal
# Do network render plot and then 
# saved the generated image file
:> render.Plot(canvasSize = [600,400]) 
:> save.graphics(file = `${!script$dir}/orthogonal.png`);