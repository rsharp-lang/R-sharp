imports "stats.clustering" from "R.math.dll";

require(dataframe);

print(ls());
setwd(!script$dir);

let input.matrix as string = ?"--data" || "bezdekIris.csv";
let centers as integer = ?"--centers"  || 4;
let result = input.matrix
:> read.dataframe(mode = "numeric")
:> kmeans(centers = centers, parallel = TRUE, debug = !script$debug)
;

if (length(result) < 100) {
	result 
	:> summary
	:> str
	;
}

result
:> as.data.frame
:> write.csv(
	file = `${dirname(input.matrix)}/${basename(input.matrix)}.kmeans.csv`, 
	row_names = FALSE
)
;

