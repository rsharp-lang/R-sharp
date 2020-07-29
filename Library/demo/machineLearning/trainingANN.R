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

ANN.training_model(
	inputSize      = input.size(dataset),
	outputSize     = output.size(dataset),
	hiddenSize     = [200, 300, 50, 10], 
	learnRate      = 0.125, 
	momentum       = 0.9, 
	minErr         = 0.05, 
	parallel       = TRUE,
	outputSnapshot = TRUE	
)
:> configuration(softmax = FALSE, selectiveMode = TRUE)
:> set.trainingSet(dataset)
:> training(maxIterations  = 1000)
:> write.ANN_network(output)
;