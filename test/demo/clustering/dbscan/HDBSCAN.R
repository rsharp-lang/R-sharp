imports "clustering" from "MLkit";

require(charts);

const filepath as string = ?"--data" || stop("a data table is required for run clustering!");
const min_points as integer = ?"--min_points" || 6;
const min_clusters as integer = ?"--min_clusters" || 5;

let raw = read.csv(filepath, row.names = NULL, check.names = FALSE)[, ["x","y"]];
let export_dir = dirname(filepath);

print("previews of the raw input data:");
print(head(raw));

let result = hdbscan(raw, min_points = min_points, min_clusters = min_clusters);

str((result));

let table = as.data.frame(raw);
table[, "Cluster"] = sapply(rownames(table), r -> result[[r]]);

table |> head |> print;
table |> write.csv(file = `${export_dir}/${basename(filepath)}_hdbscan.csv`, row_names = FALSE);

let cluster = table[, "Cluster"];
let unique_clusters = unique(cluster);
let points = [];
let colorSet = colors("Clusters", -1) :> loop;

for(id in unique_clusters) {
	let partition = table[cluster == id, ];
	let x = as.numeric(partition[, "x"]);
	let y = as.numeric(partition[, "y"]);
	
	points = c(points, serial(x,y, name = id, color = colorSet(), ptSize = 30, alpha = 230));
}

bitmap(file = `${export_dir}/${basename(filepath)}_hdbscan.png`) {
	plot(points, 
		size = [1920,1440], 
		padding = "padding: 125px 200px 200px 200px;", 
		grid.fill = "white",
		shape = "circle"
	);
}

