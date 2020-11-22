imports "clustering" from "MLkit";
imports "charts" from "R.plot";

setwd(!script$dir);

let sampleInfo = read.csv("../../sampleInfo.csv");
let labels = sampleInfo[, "ID"];
let class = `#${as.character(sampleInfo[, "color"])}`;

class = lapply(1:length(labels), i -> class[i], names = i -> labels[i]);

str(class);

# by features
let d = read.csv("../../metabolome.txt", row_names = 1, tsv = TRUE, check_names = FALSE)
# :> t
:> dist
:> hclust
;

print(d);

d :> plot(
	class       = class, 
	size        = [3000,5000], 
	padding     = "padding: 200px 1000px 200px 200px;", 
	axis.format = "G3",
	links       = "stroke: darkblue; stroke-width: 8px; stroke-dash: dash;",
	pt.color    = "gray",
	label       = "font-style: normal; font-size: 13; font-family: Bookman Old Style;",
	ticks       = "font-style: normal; font-size: 10; font-family: Bookman Old Style;"
)
:> save.graphics("./hclust2.png")
;

# by samples
d = read.csv("../../metabolome.txt", row_names = 1, tsv = TRUE, check_names = FALSE)
:> t
:> dist
:> hclust
;

print(d);

d :> plot(
	class       = class, 
	size        = [2700,4000], 
	padding     = "padding: 200px 400px 200px 200px;", 
	axis.format = "G3",
	links       = "stroke: darkblue; stroke-width: 8px; stroke-dash: dash;",
	pt.color    = "gray",
	label       = "font-style: normal; font-size: 13; font-family: Bookman Old Style;",
	ticks       = "font-style: normal; font-size: 10; font-family: Bookman Old Style;"
)
:> save.graphics("./hclust_samples.png")
;