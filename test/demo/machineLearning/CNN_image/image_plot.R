require(graphics);
require(CNN);

setwd(@dir);

const img_src = "../../1537192287563.jpg";
const raster = as.raster(img = readImage(img_src));
const encoder = CNN::cnn(file = "./img_regression.cnn");
const img = as.data.frame(raster);
const rgb = encoder(img, is_generative = TRUE);

colnames(rgb) = ["r","g","b"];

rgb[, "x"] = img$x;
rgb[, "y"] = img$y;

print(rgb, max.print = 13);

bitmap(file = "./plot.png") {
    image(rgb);
}