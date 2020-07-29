imports ["machineLearning", "machineLearning.validation"] from "R.math";

setwd(!script$dir);

let trainingSet = as.object(read.ML_model("./test.Xml"));
let ANN = "./test.trainingResult" 
:> read.ANN_network 
:> as.ANN
;

let matrix = trainingSet$NormalizeMatrix;

str(ANN :> ANN.predict(matrix :> normalize([1,1,1,1,1,1,1,1,1,1]))); # [1,1,1]
str(ANN :> ANN.predict(matrix :> normalize([1,1,1,1,1,1,1,1,1,0]))); # [0,0,1]
str(ANN :> ANN.predict(matrix :> normalize([0,1,1,1,1,1,1,1,1,1]))); # [0,0,0]

let validateSet = trainingSet 
:> raw_samples 
:> projectAs(as.object) 
:> projectAs(function(sample) {
	matrix :> as.validation(sample$vector, sample$target);
})
;

let ROC = NULL;
let aucValue as double;

for(attr in 0:2) {
	print(`ROC for predicts attribute ${attr}:`);
	
	ROC = ANN :> ANN.ROC(validateSet, [0, 1], attr);
	aucValue = AUC(ROC);
	
	print(as.data.frame(ROC));
	print(`the AUC value of the ROC result is: ${aucValue}`);
}

