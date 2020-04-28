# http content type is image/png

imports "stats.clustering" from "R.math";
imports "plot.charts" from "R.plot";

let raw = read.csv(?"file")[, ["x","y"]];
let result = as.object(dbscan(raw, 1.125))$cluster;
let table = as.data.frame(result);
let cluster = table[, "Cluster"];
let unique_clusters = unique(cluster);
let points = [];
let colorSet = colors("Clusters", -1) :> loop;

for(id in unique_clusters) {
	let partition = table[cluster == id, ];
	let x = as.numeric(partition[, "x"]);
	let y = as.numeric(partition[, "y"]);
	
	points = points << serial(x,y, name = id, color = colorSet(), ptSize = 30, alpha = 200);
}

points
:> plot(padding = "padding: 150px 100px 200px 200px;")
:> save.graphics()
;

