# script demo for read mnist dataset in raw

imports "dataset" from "MLkit";

setwd(@dir);

let raw = read.MNIST("mnist_dataset/train-images-idx3-ubyte", 
format = "mnist", 
dataset = "vector", 
labelfile = "mnist_dataset/train-labels-idx1-ubyte",
subset = 10
);

str(raw);

