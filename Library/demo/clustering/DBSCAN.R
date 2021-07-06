imports "clustering" from "MLkit";

require(charts);

setwd(!script$dir);

let raw = read.csv("multishapes.csv")[, ["x","y"]];

print("previews of the raw input data:");
print(head(raw));

let result = as.object(dbscan(raw, 0.15))$cluster;

str(summary(result));

let table = as.data.frame(result);

table :> head :> print;
table :> write.csv(file = "multishapes_dbscan.csv", row_names = FALSE);

let cluster = table[, "Cluster"];
let unique_clusters = unique(cluster);
let points = [];
let colorSet = colors("Clusters", -1) :> loop;

for(id in unique_clusters) {
	let partition = table[cluster == id, ];
	let x = as.numeric(partition[, "x"]);
	let y = as.numeric(partition[, "y"]);
	
	points = c(points, serial(x,y, name = id, color = colorSet(), ptSize = 30, alpha = 200));
}

bitmap(file = "multishapes_dbscan.png") {
	plot(points, padding = "padding: 150px 100px 200px 200px;");
}

