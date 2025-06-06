imports "validation" from "MLkit";

setwd(@dir);

# const data = read.csv(file = "github://rsharp-lang/R-sharp/blob/master/Library/demo/machineLearning/umap/ROC.csv");
const data = read.csv(file = "./ROC.csv", row.names = NULL);
const pred = prediction(data[, 1], data[, 2]);

str(data);
print(head(data));

print(`AUC = ${AUC(pred)}`);
print(`best threshold: ${pred$BestThreshold}`);

const simple_auc <- function(TPR, FPR){
  # inputs already sorted, best scores first 
  const dFPR <- c(diff(FPR), 0);
  const dTPR <- c(diff(TPR), 0);
  
  sum(TPR * dFPR) + sum(dTPR * dFPR) / 2;
};

print(simple_auc(
	TPR = pred$sensibility,
	FPR = pred$FPR
));
print(roc_auc_score(data$V2, data$V1));

bitmap(file = `${dirname(@script)}/ROC.png`) {
	plot(pred);
}

write.csv(as.data.frame(pred), file = "./AUC.csv");