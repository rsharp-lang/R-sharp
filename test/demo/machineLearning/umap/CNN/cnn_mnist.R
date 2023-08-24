imports ["CNN", "dataset"] from "MLkit";

setwd(@dir);

options(SIMD = "legacy");

const images_set = "../mnist_dataset/train-images-idx3-ubyte";
const raw = images_set 
|> read.MNIST( 
format = "mnist", 
dataset = "dataframe", 
labelfile = "../mnist_dataset/train-labels-idx1-ubyte",
subset = 10
);

str(raw);

let cnn = new cnn();

cnn = cnn + input_layer([28, 28])
+ conv_layer(6, [5, 5])
+ samp_layer([2, 2])
+ conv_layer(12, [5, 5])
+ samp_layer([2, 2])
+ output_layer(class.num = 10)

cnn = CNN::training(cnn, dataset = raw, labels = "label", max_loops = 50);

let labels = raw$label;
raw[, "label"] = NULL;

let result = CNN::predict(cnn, raw);

result[, "label"] = labels;

print(result);

write.csv(result, file = "./demo-test.csv", row.names = TRUE);