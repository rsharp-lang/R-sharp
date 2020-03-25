imports "machineLearning" from "R.math";

setwd(!script$dir);

using data as new.ML_model("test.Xml") {
	
	for(i in 1:5) {
		data :> add([1,1,1,1,1,1,1,1,1,1], [1,1,1])
	}
	
}