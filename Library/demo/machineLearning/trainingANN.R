imports "machineLearning" from "R.math";

let dataset = "D:\biodeep\biodeepdb_v3\Rscript\metacluster\training.XML"
:> read.ML_model
;

print("checking of the ML dataset...");
print(check.ML_model(dataset));

dataset
:> training.ANN(
	hiddenSize     = [200,300, 50, 10], 
	learnRate      = 0.1, 
	momentum       = 0.9, 
	minErr         = 0.05, 
	parallel       = TRUE,
	outputSnapshot = TRUE
)
:> write.ANN_network("D:\biodeep\biodeepdb_v3\Rscript\metacluster\ANN")
;