imports "dataset" from "MLkit";
imports "VAE" from "MLkit";

require(Matrix);
require(graphics);

options(SIMD = "legacy");

setwd(@dir);

const images_set = "../mnist_dataset/train-images-idx3-ubyte";
const raw = images_set 
|> read.MNIST( 
format = "mnist", 
dataset = "vector", 
labelfile = "../mnist_dataset/train-labels-idx1-ubyte",
subset = 1000
);
const embedding = VAE::embedding(raw,max_iteration= 10, batch_size = 30);

print(embedding);