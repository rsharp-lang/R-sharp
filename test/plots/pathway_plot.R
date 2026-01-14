imports "layouts" from "igraph";
imports "builder" from "igraph";

require(igraph);

let g = igraph::graph(
    from = c("A","B","E","E","C","A","G","G","A","I","K","J"), 
    to   = c("C","C","C","B","D","F","F","I","I","K","B","K")
);

print(g);

g = layout.orthogonal(g);

setwd(@dir);

pdf(file = "cor_net.pdf") {
    plot(g);
}