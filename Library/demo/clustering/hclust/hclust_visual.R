imports "clustering" from "MLkit";
imports "charts" from "graphics";

setwd(dirname(@script));

const sampleInfo       = read.csv("../../sampleInfo.csv");
const labels as string = sampleInfo[, "ID"];
const class as string  = `#${as.character(sampleInfo[, "color"])}`;
# by samples
const d = "../../metabolome.txt"
|> read.csv(
	row.names   = 1, 
	tsv         = TRUE, 
	check.names = FALSE
)
|> t
|> dist
|> hclust
;
	
print(d);

bitmap(file = "./hclust_samples.png", size = [2100, 3300]) {
	plot(d,
		class       = lapply(1:length(labels), i -> class[i], names = labels), 
		padding     = "padding: 200px 400px 200px 200px;", 
		axis.format = "G3",
		links       = "stroke: darkblue; stroke-width: 8px; stroke-dash: dash;",
		pt.color    = "gray",
		label       = "font-style: normal; font-size: 13; font-family: Bookman Old Style;",
		ticks       = "font-style: normal; font-size: 10; font-family: Bookman Old Style;"
	);
}