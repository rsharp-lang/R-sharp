imports "machineLearning" from "R.math";

options(progress_bar = "disabled");

let inputFile as string = ?"--data" || "D:\biodeep\biodeepdb_v3\Rscript\metacluster\training.XML";
let output as string    = ?"--save" || `${dirname(inputFile)}/${basename(inputFile)}.trainingResult/`;
let dataset = inputFile
:> read.ML_model
;

print("checking of the ML dataset...");
print(check.ML_model(dataset));

print("ANN training result model will be saved at location:");
print(output);

dataset
:> training.ANN(
	hiddenSize     = [100, 300, 50, 20], 
	learnRate      = 0.125, 
	momentum       = 0.9, 
	minErr         = 0.05, 
	parallel       = TRUE,
	outputSnapshot = TRUE,
	maxIterations  = 10000
)
:> write.ANN_network(output)
;