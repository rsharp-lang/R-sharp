require(REnv);

let ds = sacurine();

str(ds);

let x = ds$dataMatrix;
let labels = ds$sampleMetadata;

print(labels);

let pls = plsda(x, y = labels[, "gender"]);

str(pls);

print(pls$component);
print(pls$scoreMN);
print(pls$loadingMN);
