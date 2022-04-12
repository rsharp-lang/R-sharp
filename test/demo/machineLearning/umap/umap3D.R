imports ["dataset", "umap"] from "MLkit";

let raw = read.csv("F:\lipids\areas_lipidmaps.csv", check_modes = FALSE, check_names = FALSE, row_names = 1);

print(rownames(raw));
print(colnames(raw));

for(col in colnames(raw)) {
	raw[, col] = as.numeric(raw[, col]);
}

let manifold = raw
:> umap(
	dimension            = 3, 
	numberOfNeighbors    = 15,
    localConnectivity    = 1,
    KnnIter              = 64,
    bandwidth            = 1,
	customNumberOfEpochs = 100
)
;

let result = as.data.frame(manifold$umap, labels = manifold$labels, dimension = ["X", "Y", "Z"]);

write.csv(result, file = "F:\lipids\areas_umap3D.csv");