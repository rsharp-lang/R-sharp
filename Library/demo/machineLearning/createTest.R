imports "machineLearning" from "R.math";

setwd(!script$dir);

using data as new.ML_model("test.Xml") {
	
	for(i in 1:50) {
		data :> add([1,1,1,1,1,1,1,1,1,1], [1,1,1])
	}
	
	for(i in 1:50) {
		data :> add([1,1,1,1,1,1,1,1,1,0], [0,0,1])
	}
	
	for(i in 1:50) {
		data :> add([0,1,1,1,1,1,1,1,1,1], [0,0,0])
	}
	
	for(i in 1:30) {
		data :> add(runif(10, 0, 0.6), [1,0,0])
	}
}