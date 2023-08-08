# script demo for read mnist dataset in raw

imports "dataset" from "MLkit";

require(Matrix);

setwd(@dir);

const images_set = "mnist_dataset/train-images-idx3-ubyte";

str(MNIST.dims(images_set));

let raw = images_set 
|> read.MNIST( 
format = "mnist", 
dataset = "vector", 
labelfile = "mnist_dataset/train-labels-idx1-ubyte",
subset = 10
);

str(raw);

str(raw[[1]]);

let m = matrix(raw[[1]], nrow = 28, byrow = TRUE);

print(m);

