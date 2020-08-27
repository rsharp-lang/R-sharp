imports ["dataset", "machineLearning", "GA_toolkit"] from "MLkit";

# demo script for running ANN model training

options(progress_bar = "disabled");

let inputFile as string    = ?"--data"   || stop("no training data set was provided!");
let output as string       = ?"--save"   || `${dirname(inputFile)}/${basename(inputFile)}_ANN/`;
let maxLoops as integer    = ?"--loops"  || 10000;
let hiddens as string      = ?"--hidden" || "120,300,200,20";
let attr as string         = ?"--attr";
let GA_run as boolean      = ?"--ga";
let dropout.rate as double = 0;
let dataset = inputFile
:> read.ML_model
;

if (attr == "") {
	attr = -1;
} else {
	attr = as.integer(attr);
	print("a single specific output attribute will be trained...");
}

print("checking of the ML dataset...");
print(check.ML_model(dataset));

print("ANN training result model will be saved at location:");
print(output);

let ANN = ANN.training_model(
	inputSize      = input.size(dataset),
	outputSize     = (attr < 0) ? output.size(dataset) : 1,
	hiddenSize     = as.integer(strsplit(hiddens, ',')), 
	learnRate      = 0.125, 
	momentum       = 0.9, 
	minErr         = 0.05, 	
	outputSnapshot = TRUE,
	truncate       = -1,
	split          = FALSE
)
:> configuration(softmax = FALSE)
:> configuration(selectiveMode = TRUE)
:> configuration(dropout = dropout.rate)
:> configuration(snapshotLocation = output)
:> set.trainingSet(dataset, attribute = attr)
;

let ANN_result = NULL;

if (GA_run) {
	print("ANN model will be training in GA framework!");

	ANN_result = ANN :> ANN.training(
		trainingSet = dataset,
		populationSize = 50
	);
} else {
	ANN_result = ANN :> training(
		maxIterations = maxLoops, 
		parallel      = TRUE
	);
}

ANN_result
:> write.ANN_network(output)
;