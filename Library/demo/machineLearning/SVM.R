imports "SVM" from "MLkit";
imports "JSON" from "R.base";

let svm = svm.problem(["X", "Y", "Z"])
:> append.trainingSet(
	tag = 10,
	data = data.frame(X = runif(20, -120, -100), Y = runif(20, 1, 2), Z = runif(20, 10, 12))
)
:> append.trainingSet(
	tag = 2,
	data = data.frame(X = runif(20, 1, 10), Y = runif(20, 0, 20), Z = runif(20, 10, 12))
)
:> append.trainingSet(
	tag = 3,
	data = data.frame(X = runif(20, 300, 500), Y = runif(20, 300, 310), Z = runif(20, 10, 120))
)
:> append.trainingSet(
	tag = "332A",
	data = data.frame(X = runif(20, 300, 500), Y = runif(20, 1300, 1310), Z = runif(20, 10, 512))
)
:> trainSVMModel
;

# 2 10 2 3 332A 2
let validates = data.frame(X = [2,-103,3,311,500, 50], Y = [1,2,1.3,302,1800, 50], Z = [2,-103,3,311,500, 50]);

rownames(validates) = ["a","b","c","d","e", "f"];

str(svm :> svm_classify(validates));

const json_saved = `${!script$dir}/SVM.json`;

print(`the svm model in json format will be saved at location: ${json_saved}`);

svm 
:> svm_json
:> writeLines(con = json_saved)
;

print(svm :> svm_json);

let bson_file as string = `${!script$dir}/SVM.bson`;

svm :> svm_json(fileModel = TRUE) :> write.bson(file = bson_file);

print("validate result from the json model loaded result:");

json_saved
:> readText
:> parse.SVM_json
:> svm_classify(validates)
:> str
;

print("validate result from the bson model loaded result:");

using model as file(bson_file) {
	svm = model 
	:> parseBSON(raw = TRUE) 
	:> parse.SVM_json 
	;
	
	print(svm :> svm_json);
	
	svm
	:> svm_classify(validates)
	:> str
	;
}