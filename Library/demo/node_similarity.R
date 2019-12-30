imports ["igraph", "igraph.comparison"] from "R.graph.dll";

let g = read.network(`${!script$dir}/demo_network`);
let a = g :> getElementByID("B1");
let b = g :> getElementByID("B1");

print(node.cos(a,b));