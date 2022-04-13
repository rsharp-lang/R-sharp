imports "SVM" from "MLkit";
imports "JSON" from "R.base";

let trainingSet = ?"--data" || stop("no SVM trainingSet data was provided!");
let save_bson   = ?"--save" || `${dirname(trainingSet)}/${basename(trainingSet)}.svm.bson`;

trainingSet 
:> readText 
:> parse.SVM_problems
:> trainSVMModel(verbose = TRUE)
:> svm_json(fileModel = TRUE)
:> write.bson(file = save_bson)
;
