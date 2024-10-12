imports "validation" from "MLkit";

setwd(@dir);

# const data = read.csv(file = "github://rsharp-lang/R-sharp/blob/master/Library/demo/machineLearning/umap/ROC.csv");
let data = read.csv(file = "./ROC.csv", row.names = NULL);
let labels = data$V2;
let accu = 0.9;
let result = sapply(labels, function(xi) {
    if (runif() < accu) {
        xi;
    } else {
        1-xi;
    }
});

const pred = prediction(result, labels);

str(data);
print(head(data));

print(`AUC = ${AUC(pred)}`);
print(`best threshold: ${pred$BestThreshold}`);

bitmap(file = "./ROC2.png") {
	plot(pred);
}

write.csv(as.data.frame(pred), file = "./AUC2.csv");