require(graphics);
require(CNN);

setwd(@dir);

const img_src = "../../1537192287563.jpg";
const raster = as.raster(img = readImage(img_src));
const data = raster_convolution(raster);
const ds = CNN::sample_dataset(data, labels = ["r","g","b"]);

print(data);

let encoder = CNN::cnn()
+ input_layer(size = [1,1], depth = 3)
+ full_connected_layer(20)
+ full_connected_layer(20)
+ full_connected_layer(20)
+ full_connected_layer(20)
+ full_connected_layer(20)
+ full_connected_layer(20)
+ full_connected_layer(20)
+ full_connected_layer(3)
+ regression_layer()
;

encoder = CNN::training(cnn = encoder, dataset = ds);