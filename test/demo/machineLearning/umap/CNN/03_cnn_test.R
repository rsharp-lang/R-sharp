imports ["dataset", "CNN"] from "MLkit";

setwd(@dir);

let ds = read.ML_model(file = "./images_x4.pack");
ds = as.sampleSet(ds);

print(CNN::n_threads());

let cnn = cnn();

cnn = cnn + input_layer([28, 28])
+ conv_layer(5, 32, 1, 2)
+ pool_layer(2, 2, 0)
+ conv_layer(5, 64, 1, 2)
+ pool_layer(2, 2, 0)
+ full_connected_layer(4)
+ relu_layer()
;

CNN::n_threads(8);
cnn = CNN::training(cnn, dataset = ds, max.loops = 6, trainer = CNN::ada_grad(batch.size = 50));