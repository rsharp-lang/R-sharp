imports ["dataset", "umap"] from "MLkit";

let raw = read.csv(file.path(@dir, "vector.csv"), check_modes = FALSE, check_names = FALSE, row_names = 1);

print(rownames(raw));
print(colnames(raw));

for(col in colnames(raw)) {
	raw[, col] = as.numeric(raw[, col]);
}

let manifold = raw
|> umap(
	dimension            = 3, 
	numberOfNeighbors    = 15,
    localConnectivity    = 1,
    KnnIter              = 64,
    bandwidth            = 1,
	customNumberOfEpochs = 100
)
;

let result = as.data.frame(manifold$umap, labels = manifold$labels, dimension = ["X", "Y", "Z"]);
let class = strsplit(manifold$labels, "-",fixed = TRUE);

result[, "class"] = class@{1};

print(result);

write.csv(result, file = file.path(@dir, "umap3D.csv"));

bitmap(file = file.path(@dir, "visual.png"), size = [3600,2700]) {
	plot(result$X, result$Y,
		class = result$class,
		show_bubble = FALSE,
		point_size  = 50,
		legendlabel = "font-style: normal; font-size: 24; font-family: Bookman Old Style;",
		padding     = "padding:150px 350px 350px 350px;",
		fill = "white"
	);
}