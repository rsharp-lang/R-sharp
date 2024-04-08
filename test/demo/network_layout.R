require(igraph);

imports ["layouts" "visualizer"] from "igraph";

# let test.network as string = `${!script$dir}/demo_network`;
let test = read.csv(file.path(@dir, "renderNetwork", "correlation_dem_22.csv"), 
    row.names = NULL, check.names = FALSE);
let g = igraph::graph(
    test$compoundA, test$compoundB,
    test$correlation
);

# create a new empty network graph model
# let g = empty.network()
# :> add.nodes(labels = ["A","B","C","D"]);

# g :> add.edge("A", "B");
# g :> add.edge("A", "C");
# g :> add.edge("C", "D");
# g :> add.edge("D", "B");

# view summary info of the network
print(g);

bitmap(file = file.path(@dir, "orthogonal.png")) {
    g
    |> layout.orthogonal(layoutIteration = 10)
    # Do network render plot and then 
    # saved the generated image file
    |> render(canvasSize = [2000,1400]) 
    ;
}