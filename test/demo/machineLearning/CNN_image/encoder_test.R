imports ["CNN", "dataset"] from "MLkit";

setwd(@dir);

options(SIMD = "legacy");

const images_set = "../umap/mnist_dataset/train-images-idx3-ubyte";
const raw = images_set 
|> read.MNIST( 
format = "mnist", 
dataset = "dataframe", 
labelfile = "../umap/mnist_dataset/train-labels-idx1-ubyte",
subset = 600
);

const decoder = CNN::cnn(file = "./MNIST_autoencoder.cnn");

print(decoder);

