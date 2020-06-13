imports "igraph" from "R.graph";

let g = (new graph())
:> add.nodes(["1","2","3","4","5","6", "7", "8","9","10","11","12"])
;

g :> add.edge(4,1);
g :> add.edge(2,3);
g :> add.edge(3,2);
g :> add.edge(4,3);
g :> add.edge(6,3);
g :> add.edge(6,4);
g :> add.edge(6,5);
g :> add.edge(5,7);
g :> add.edge(7,6);
g :> add.edge(9,7);
g :> add.edge(9,8);
g :> add.edge(8,10);
g :> add.edge(10,9);
g :> add.edge(11,8);
g :> add.edge(11,10);
g :> add.edge(12,11);   

g 
:> decompose