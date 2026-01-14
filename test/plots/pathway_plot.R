imports "layouts" from "igraph";
imports "builder" from "igraph";

require(igraph);

let expr = read.csv("G:\GCModeller\src\workbench\pkg\test\zero_test\expression_matrix.csv", row.names = 1, check.names = FALSE);
let g = corr(expr) |> correlation.graph(threshold = 0.85);

print(g);

g = layout.orthogonal(g);

setwd(@dir);

pdf(file = "cor_net.pdf") {
    plot(g);
}