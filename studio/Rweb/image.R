# http content type is image/png

imports "stats.clustering" from "R.math";
imports "plot.charts" from "R.plot";

let raw = read.csv(?"file")[, ["x","y"]];
let result = as.object(dbscan(raw, 1.125))$cluster;
let table = as.data.frame(result);
let colorSet = colors("Clusters", -1) :> loop;

table[, "Cluster"]
:> unique
:> sapply(function(id) {
	let partition = table[cluster == id, ];
	let x = as.numeric(partition[, "x"]);
	let y = as.numeric(partition[, "y"]);
	
	serial(x,y, name = id, color = colorSet(), ptSize = 30, alpha = 200);
})
:> as.vector
:> plot(padding = "padding: 150px 100px 200px 200px;")
:> save.graphics()
;

