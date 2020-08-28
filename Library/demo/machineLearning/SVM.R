imports "SVM" from "MLkit";

let svm = svm.problem(["X", "Y"])
:> append.trainingSet(
	tag = -1,
	data = data.frame(X = runif(100, -120, -100), Y = runif(100,1,2))
)
:> append.trainingSet(
	tag = 2,
	data = data.frame(X = runif(100, 1, 10), Y = runif(100, 0, 20))
)
:> append.trainingSet(
	tag = 3,
	data = data.frame(X = runif(100, 300, 500), Y = runif(100, 300, 310))
)
:> trainSVMModel
;

# 2 -1 2 3
let validates = data.frame(X = [2,-103,3,311], Y = [1,2,1.3,302]);

rownames(validates) = ["a","b","c","d"];

print(svm :> svm_classify(validates));