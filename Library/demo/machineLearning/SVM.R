imports "SVM" from "MLkit";

let svm = svm.problem(["X", "Y", "Z"])
:> append.trainingSet(
	tag = 1,
	data = data.frame(X = [1,1,1], Y = [1,1,1], Z = [1,1,1])
)
:> append.trainingSet(
	tag = 2,
	data = data.frame(X = [2,2,2], Y = [2,2,2], Z = [2,2,2])
)
:> append.trainingSet(
	tag = 3,
	data = data.frame(X = [3,3,3], Y = [3,3,3], Z = [3,3,3])
)
:> trainSVMModel
;

let validates = data.frame(X = [1,2,3,4], Y = [1,2,3,4], Z = [1,2,3,4]);

rownames(validates) = ["a","b","c","d"];

print(svm :> svm_classify(validates));