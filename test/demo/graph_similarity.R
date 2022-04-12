imports ["igraph", "igraph.comparison"] from "R.graph.dll";

let g = read.network(`${!script$dir}/demo_network`);
let z = read.network(`${!script$dir}/demo_network`);

# z is identical to g
print(graph.jaccard(g, z));

# modify z network
g :> add.edge("G", "E");
g :> add.edge("G", "C1");
g :> add.edge("G", "B1");
g :> add.edge("Z", "D");

# g :> add.node("ABCDEFG");

# z is similar to g
print(graph.jaccard(g, z, 0.9));