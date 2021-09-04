imports "validation" from "MLkit";

setwd(@dir);

const data = read.csv(file = "test_result2.csv", row.names = NULL);
const pred = prediction(data[, "predict"], data[, "label"]);

str(data);
print(head(data));

print(`AUC = ${AUC(pred)}`);

const simple_auc <- function(TPR, FPR){
  # inputs already sorted, best scores first 
  const dFPR <- c(diff(FPR), 0);
  const dTPR <- c(diff(TPR), 0);
  
  sum(TPR * dFPR) + sum(dTPR * dFPR) / 2;
};

# print(simple_auc(
	# TPR = pred$sensibility,
	# FPR = pred$FPR
# ));

bitmap(file = `${dirname(@script)}/ROC.png`) {
	plot(pred);
}