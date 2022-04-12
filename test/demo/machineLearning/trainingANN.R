imports ["dataset", "machineLearning", "GA_toolkit"] from "MLkit";

# title: ANN model training
#
# description: demo script for running ANN model training
#
# author: xieguigang

options(progress_bar = "disabled");

[@info "the training data XML dataset file its file path."]
let inputFile as string    = ?"--data"   || stop("no training data set was provided!");
[@info "the save location of the result network model, for missing parameter will save at filename_ANN dir by default"]
let output as string       = ?"--save"   || `${dirname(inputFile)}/${basename(inputFile)}_ANN/`;
[@info "max number of iterations for training ANN."]
let maxLoops as integer    = ?"--loops"  || 10000;
[@info "the layer numbers and the layer size of the hidden layers of your ANN model"]
[@type "vector"]
let hiddens as string      = ?"--hidden" || "120,300,200,20";
[@info "select a single specific output attribute will be trained..."]
let attr as string         = ?"--attr";
let split_out as boolean   = ?"--split";
let GA_run as boolean      = ?"--ga";
let GA_popSize as integer  = ?"--ga.pop_size" || 250;
[@info "the rate of random dropout."]
let dropout.rate as double = ?"--dropout" || 0;
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

if (split_out) {
	print("ANN model output will be split into multiple partitions...");
}

let ANN = ANN.training_model(
	inputSize      = input.size(dataset),
	outputSize     = (attr < 0) ? output.size(dataset) : 1,
	hiddenSize     = as.integer(strsplit(hiddens, ',')), 
	learnRate      = 0.125, 
	momentum       = 0.9, 
	minErr         = 0.05, 	
	outputSnapshot = TRUE,
	truncate       = -1,
	split          = split_out
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
		trainingSet    = dataset,
		populationSize = GA_popSize,
		iterations     = maxLoops 
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