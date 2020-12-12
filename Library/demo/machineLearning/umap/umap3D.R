imports ["dataset", "umap"] from "MLkit";

let manifold = read.csv("F:\lipids\areas.csv")
:> umap(
	dimension         = 3, 
	numberOfNeighbors = 15,
    localConnectivity = 1,
    KnnIter           = 64,
    bandwidth         = 1
)
;

let result = as.data.frame(manifold$umap, labels = manifold$labels, dimension = ["X", "Y", "Z"]);

write.csv(result, file = "F:\lipids\areas_umap3D.csv");