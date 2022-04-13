imports ["igraph", "igraph.dijkstra"] from "R.graph.dll";

setwd(!script$dir);

read.network("demo_network")
:> router.dijkstra(undirected = TRUE)
:> routine.min_cost("G", "F1")
:> print;