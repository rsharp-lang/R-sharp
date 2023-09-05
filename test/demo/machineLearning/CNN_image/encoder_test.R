imports ["CNN", "dataset"] from "MLkit";

setwd(@dir);

options(SIMD = "legacy");

const images_set = "../umap/mnist_dataset/train-images-idx3-ubyte";
const raw = images_set 
|> read.MNIST( 
format = "mnist", 
dataset = "dataframe", 
labelfile = "../umap/mnist_dataset/train-labels-idx1-ubyte",
subset = 300
);

const decoder = CNN::cnn(file = "./MNIST_autoencoder.cnn");

print(decoder);

raw[,"label"] = NULL;

const images = decoder(raw);

require(Matrix);
require(graphics);

const plots = as.list(images, byrow = TRUE)
|> lapply(function(l) {
    matrix(as.numeric(unlist(l)), byrow = TRUE, ncol = 28, nrow = 28);
});

for(name in names(plots)) {
    bitmap(file = `./decoder_plot/${name}.png`) {
         image(plots[[name]]);
    }
}