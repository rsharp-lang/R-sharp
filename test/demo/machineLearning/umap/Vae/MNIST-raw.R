imports "dataset" from "MLkit";
imports "VAE" from "MLkit";

require(Matrix);
require(graphics);

setwd(@dir);

const images_set = "../mnist_dataset/train-images-idx3-ubyte";
const raw = images_set 
|> read.MNIST( 
format = "mnist", 
dataset = "vector", 
labelfile = "../mnist_dataset/train-labels-idx1-ubyte",
subset = 10
);
const vae = VAE::vae(dims = MNIST.dims(images_set));
const encoder = VAE::train(vae, ds = raw);

for(id in names(raw)) {
    
}