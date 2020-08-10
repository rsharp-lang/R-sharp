imports "machineLearning" from "MLkit";

options(progress_bar = "disabled");

let inputFile as string = ?"--data"   || stop("no training data set was provided!");
let output as string    = ?"--save"   || `${dirname(inputFile)}/${basename(inputFile)}.trainingResult/`;
let maxLoops as integer = ?"--loops"  || 10000;
let hiddens as string   = ?"--hidden" || "120,300,200,20";
let attr as string      = ?"--attr";
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

ANN.training_model(
	inputSize      = input.size(dataset),
	outputSize     = (attr < 0) ? output.size(dataset) : 1,
	hiddenSize     = as.integer(strsplit(hiddens, ',')), 
	learnRate      = 0.125, 
	momentum       = 0.9, 
	minErr         = 0.05, 
	parallel       = TRUE,
	outputSnapshot = TRUE	
)
:> configuration(softmax = FALSE, selectiveMode = TRUE)
:> set.trainingSet(dataset, attribute = attr)
:> training(maxIterations  = maxLoops)
:> write.ANN_network(output)
;