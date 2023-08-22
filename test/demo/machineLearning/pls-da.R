require(REnv);

let ds = sacurine();

str(ds);

let x = ds$dataMatrix;
let labels = ds$sampleMetadata;

print(labels);

pls = plsda(x, y = labels[, "gender"], ncomp = 3);

str(pls);