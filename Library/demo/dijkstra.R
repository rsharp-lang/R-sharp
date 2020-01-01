imports ["igraph", "igraph.dijkstra"] from "R.graph.dll";

setwd(!script$dir);

read.network("demo_network")
:> router.dijkstra
:> routine.min_cost("", "")
:> print;