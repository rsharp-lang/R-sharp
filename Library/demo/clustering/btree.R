require(dataframe);

let d = t(as.data.frame(read.dataframe("F:\lipids\areas_lipidmaps.csv", mode = "numeric")));

print("colnames:");
print(colnames(d));
print("rownames:");
print(rownames(d));

for(lipid in colnames(d)) {
	d[, lipid] = d[, lipid] / max(d[, lipid]); 
}

d = sim(t(d));

print(d);

