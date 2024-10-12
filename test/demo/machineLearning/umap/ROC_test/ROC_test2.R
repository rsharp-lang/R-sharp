imports "validation" from "MLkit";

setwd(@dir);

# const data = read.csv(file = "github://rsharp-lang/R-sharp/blob/master/Library/demo/machineLearning/umap/ROC.csv");
let data = read.csv(file = "./ROC.csv", row.names = NULL);
let labels = data$V2;
let accu = 0.8;
let result = fake_result(labels, accu);
let pred = prediction(result, labels);

print(data.frame(result, labels));

print(roc_auc_score(labels, result));
print(roc_auc_score(labels, 1 - result));

print(`AUC = ${AUC(pred)}`);
print(`best threshold: ${pred$BestThreshold}`);

bitmap(file = "./ROC2.png") {
	plot(pred);
}

write.csv(as.data.frame(pred), file = "./AUC2.csv");
write.csv(data.frame(result, labels), file = "./generates.csv");