i = strHashcode("Hello World!");

if (i != 1726978645) {
	stop("the string hash code is not deterministic!");
} else {
	print("string hash code test success!");
}

# unique hash code test
dataset = read.csv(`${@dir}/metadata.csv`, row.names = 1);

print("has data fields in meta dataset:");
print(colnames(dataset));

index = dataset[, ["cas", "kegg", "formula", "inchikey", "hmdb","chebi","lipidmaps"]];

cat("\n\n");

print("view content index data:");
print(index);

hashSet = for(obj in as.list(index, byrow = TRUE)) {
	toString(abs(FNV1aHashcode(obj)), format ="F0");
}

cat("\n\n");

print("get unique reference id of the metadataset:");
print(`BioDeep_${str_pad(hashSet, 10, pad = "0")}`);

# [1]     "BioDeep_0575054657" "BioDeep_1600502933" "BioDeep_0835306603"
# [4]     "BioDeep_0806957015" "BioDeep_0200517077" "BioDeep_0123575871"
# [7]     "BioDeep_1261152084" "BioDeep_1297237314" "BioDeep_0003467586"
# [10]    "BioDeep_1436769182" "BioDeep_0131846530"