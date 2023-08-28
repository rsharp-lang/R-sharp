imports ["dataset", "CNN"] from "MLkit";

setwd(@dir);

let ds = read.ML_model(file = "./images_x4.pack");
ds = as.sampleSet(ds);

let cnn = cnn();

cnn = cnn + input_layer([56, 56])
+ conv_layer(6, [5, 5])
+ pool_layer([2, 2])
+ conv_layer(12, [5, 5])
+ pool_layer([2, 2])
+ output_layer(class.num = 10, w = 4)
;

cnn = CNN::training(cnn, dataset = ds, max.loops = 6, batch.size = 50);