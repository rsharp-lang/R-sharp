require(graphics);
require(CNN);

setwd(@dir);

const img_src = "../../1537192287563.jpg";
const raster = as.raster(img = readImage(img_src));
const data = as.data.frame(raster, rgb = TRUE);

# data[, "scale"] = NULL;

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
+ full_connected_layer(20)
+ relu_layer()
+ full_connected_layer(20)
+ relu_layer()
+ full_connected_layer(20)
+ relu_layer()
+ full_connected_layer(3)
+ regression_layer()
;

const img = data.frame(x = data$x, y = data$y, scale = data$scale);

encoder = CNN::training(cnn = encoder, dataset = ds, max_loops = 50, 
    algorithm = CNN::sgd(batch_size = 5), 
    action = function(t, cnn) {
        const rgb = CNN::predict(cnn, img, is_generative = TRUE);

        colnames(rgb) = ["r","g","b"];

        rgb[, "x"] = img$x;
        rgb[, "y"] = img$y;

        print(rgb, max.print = 13);

        bitmap(file = `./plot_${t+1}.png`) {
            image(rgb);
        }
    });

CNN::saveModel(encoder, file = "./img_regression.cnn");


