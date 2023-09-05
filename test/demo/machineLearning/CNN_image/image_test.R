require(graphics);
require(CNN);

setwd(@dir);

const img_src = "../../1537192287563.jpg";
const raster = as.raster(img = readImage(img_src));
const data = raster_convolution(raster);
const ds = CNN::sample_dataset(data, labels = ["r","g","b"]);

print(data, max.print = 6);

let encoder = CNN::cnn()
+ input_layer(size = [1,1], depth = 3)
+ full_connected_layer(20)
+ relu_layer()
+ full_connected_layer(20)
+ relu_layer()
+ full_connected_layer(20)
+ relu_layer()
+ full_connected_layer(20)
+ relu_layer()
+ full_connected_layer(80)
+ relu_layer()
+ full_connected_layer(20)
+ relu_layer()
+ full_connected_layer(20)
+ relu_layer()
+ full_connected_layer(3)
+ relu_layer()
+ regression_layer()
;

encoder = CNN::training(cnn = encoder, dataset = ds, max_loops = 5,  trainer = CNN::sgd(batch_size = 130));

CNN::saveModel(encoder, file = "./img_regression.cnn");