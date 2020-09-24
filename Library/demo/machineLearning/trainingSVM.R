imports "SVM" from "MLkit";

let trainngSet = ?"--data" || stop("no SVM trainingSet data was provided!");
let save_json = ?"--save" || `${dirname(trainingSet)}/${basename(trainingSet)}.svm.json`;

let svm_model = trainingSet 
:> readText 
:> parse.SVM_problems
:> trainSVMModel
;

svm_model
:> svm_json
:> writeLines(con = save_json)
;
