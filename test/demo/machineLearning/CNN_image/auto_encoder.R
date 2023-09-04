imports ["CNN", "dataset"] from "MLkit";

setwd(@dir);

options(SIMD = "legacy");

const images_set = "../umap/mnist_dataset/train-images-idx3-ubyte";
const raw = images_set 
|> read.MNIST( 
format = "mnist", 
dataset = "dataframe", 
labelfile = "../umap/mnist_dataset/train-labels-idx1-ubyte",
subset = 60
);

let cnn = cnn() 
+ input_layer([28, 28], 1)
+ conv_layer(5, 32, 1, 2)
+ pool_layer(2, 2, 0)
+ leaky_relu_layer()
+ conv_layer(5, 5, 1, 2)
+ pool_layer(2, 2, 0)
+ sigmoid_layer()
+ conv_transpose_layer([7,7,5], [3,3],2,1)
+ leaky_relu_layer()
+ conv_transpose_layer([28,28,1],[10,10],5,3)
+ relu_layer()
;

let ds = sample_dataset(dataset = raw, labels = as.numeric(labels));
let encoder = CNN::auto_encoder(cnn, ds, max.loops = 3, trainer = CNN::ada_grad(batch.size = 60));

print(encoder);

CNN::saveModel(encoder, file = "./MNIST_autoencoder.cnn");