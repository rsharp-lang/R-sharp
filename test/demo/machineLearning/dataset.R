imports "machineLearning" from "R.math";

let file as string = ?"--data" || stop("no data provided!");

file
:> read.ML_model
:> as.tabular
:> write.csv(file = `${dirname(file)}/${basename(file)}.dataset.csv`)
;