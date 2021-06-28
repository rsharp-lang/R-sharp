imports "validation" from "MLkit";

const data = read.csv(file = `${dirname(@script)}/ROC.csv`);
const pred = prediction(data[, 1], data[, 2]);

print(as.data.frame(pred));
print(`AUC = ${AUC(pred)}`);