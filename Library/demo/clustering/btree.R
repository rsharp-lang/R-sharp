imports "clustering" from "MLkit";
 
require(dataframe);

let d = t(as.data.frame(read.dataframe("F:\lipids\areas_lipidmaps.csv", mode = "numeric")));

print("colnames:");
print(colnames(d));
print("rownames:");
print(rownames(d));

# data normalization
for(lipid in colnames(d)) {
	d[, lipid] = d[, lipid] / max(d[, lipid]); 
}

# create a similarity matrix
d = sim(t(d));

print(d);

btree(d, equals = 0.9, gt = 0.75)
:> json
:> writeLines(con = "F:\lipids\lipidsearch.json")
;

