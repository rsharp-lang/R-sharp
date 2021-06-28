imports "validation" from "MLkit";

const data = read.csv(file = "github://rsharp-lang/R-sharp/blob/master/Library/demo/machineLearning/umap/ROC.csv");
const pred = prediction(data[, 1], data[, 2]);

print(as.data.frame(pred));
print(`AUC = ${AUC(pred)}`);