require(charts);

setwd(dirname(@script));

const raw = read.csv("../feature_regions_dbscan.csv", check.names = FALSE, row.names = FALSE);

print(head(raw));

const render2D = lapply(raw[, "ID"], function(pstr) {
	pstr 
	|> strsplit(",", fixed = TRUE)
	|> unlist
	|> as.integer
	;
});

const x = sapply(render2D, p -> p[1]);
const y = sapply(render2D, p -> p[2]);
const cluster = raw[, "Cluster"];

str(x);
str(y);

bitmap(file = "./render_cluster2D.png") {
	plot(
		x, y, class = cluster, grid.fill = "white", reverse = TRUE, shape        = "Square",point_size   = 25,colorSet = "Set1:c8"
	);
}
