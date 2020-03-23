imports "machineLearning" from "R.math";

options(progress_bar = "disabled");

let inputFile as string = ?"--data" || "D:\biodeep\biodeepdb_v3\Rscript\metacluster\training.XML";
let output as string    = ?"--save" || `${dirname(inputFile)}/${basename(inputFile)}.ANN_trained/`;
let dataset = inputFile
:> read.ML_model
;

print("checking of the ML dataset...");
print(check.ML_model(dataset));

dataset
:> training.ANN(
	hiddenSize     = [1000, 3000, 500, 300], 
	learnRate      = 0.125, 
	momentum       = 0.9, 
	minErr         = 0.05, 
	parallel       = TRUE,
	outputSnapshot = TRUE,
	maxIterations  = 10000
)
:> write.ANN_network(output)
;