imports "machineLearning" from "R.math";

setwd(!script$dir);

let trainingSet = as.object(read.ML_model("./test.Xml"))$NormalizeMatrix;
let ANN = "./test.trainingResult" 
:> read.ANN_network 
:> as.ANN
;

str(ANN :> ANN.predict(trainingSet :> normalize([1,1,1,1,1,1,1,1,1,1])));
str(ANN :> ANN.predict(trainingSet :> normalize([1,1,1,1,1,1,1,1,1,0])));
str(ANN :> ANN.predict(trainingSet :> normalize([0,1,1,1,1,1,1,1,1,1])));