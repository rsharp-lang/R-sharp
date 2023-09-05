require(graphics);
require(CNN);

setwd(@dir);

const img_src = "../../1537192287563.jpg";
const raster = as.raster(img = readImage(img_src));
const encoder = CNN::cnn(file = "./img_regression.cnn");
const img = as.data.frame(raster);
const rgb = encoder(img, is_generative = TRUE);

print(rgb, max.print = 13);

