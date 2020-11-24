imports "clustering" from "MLkit";
imports "charts" from "R.plot";

setwd(!script$dir);

# by features
let d = read.csv("../../metabolome.txt", row_names = 1, tsv = TRUE, check_names = FALSE)
# :> t
:> dist
:> btree(hclust = TRUE)
;

print(d);

d :> plot(	
	size        = [3000,5000], 
	padding     = "padding: 200px 1000px 200px 200px;", 
	axis.format = "G3",
	links       = "stroke: darkblue; stroke-width: 8px; stroke-dash: dash;",
	pt.color    = "gray",
	label       = "font-style: normal; font-size: 13; font-family: Bookman Old Style;",
	ticks       = "font-style: normal; font-size: 10; font-family: Bookman Old Style;"
)
:> save.graphics("./btree.png")
;