require(graphics);
require(CNN);

setwd(@dir);

const img_src = "../../1537192287563.jpg";
const raster = as.raster(img = readImage(img_src));
const data = as.data.frame(raster, rgb = TRUE);

data[, "scale"] = NULL;

const ds = CNN::sample_dataset(data, labels = ["r","g","b"]);

print(data, max.print = 6);

let encoder = CNN::cnn()
+ input_layer(size = [1,1], depth = 2)
+ full_connected_layer(20)
+ relu_layer()
+ full_connected_layer(20)
+ relu_layer()
+ full_connected_layer(20)
+ relu_layer()
+ full_connected_layer(20)
+ relu_layer()
+ full_connected_layer(20)
+ relu_layer()
+ full_connected_layer(20)
+ relu_layer()
+ full_connected_layer(20)
+ relu_layer()
+ full_connected_layer(3)
+ regression_layer()
;

encoder = CNN::training(cnn = encoder, dataset = ds, max_loops = 20,  trainer = CNN::sgd(batch_size = 5));

CNN::saveModel(encoder, file = "./img_regression.cnn");

const img = data.frame(x = data$x, y = data$y);
const rgb = encoder(img, is_generative = TRUE);

colnames(rgb) = ["r","g","b"];

rgb[, "x"] = img$x;
rgb[, "y"] = img$y;

print(rgb, max.print = 13);

bitmap(file = "./plot.png") {
    image(rgb);
}