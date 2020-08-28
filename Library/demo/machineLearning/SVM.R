imports "SVM" from "MLkit";

let svm = svm.problem(["X", "Y", "Z"])
:> append.trainingSet(
	tag = 1,
	data = data.frame(X = [-100,-100,-100], Y = [1,1,1], Z = [1,1,1])
)
:> append.trainingSet(
	tag = 2,
	data = data.frame(X = [2,2,2], Y = [2,2,2], Z = [2,2,2])
)
:> append.trainingSet(
	tag = 3,
	data = data.frame(X = [300,300,300], Y = [300,300,300], Z = [300,300,300])
)
:> trainSVMModel
;

let validates = data.frame(X = [1,-200,3,400], Y = [1,2,3,400], Z = [1,2,3,400]);

rownames(validates) = ["a","b","c","d"];

print(svm :> svm_classify(validates));