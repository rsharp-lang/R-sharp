imports ["CNN", "dataset"] from "MLkit";

setwd(@dir);

options(SIMD = "legacy");

const images_set = "../mnist_dataset/train-images-idx3-ubyte";
const raw = images_set 
|> read.MNIST( 
format = "mnist", 
dataset = "dataframe", 
labelfile = "../mnist_dataset/train-labels-idx1-ubyte",
subset = 20
);

str(raw);

let labels = raw$label;

raw[, "label"] = NULL;

for(name in colnames(raw)) {
    let v = as.numeric(raw[, name]);

    if (sum(v) > 0) {
        v = v / max(v);
        raw[, name] = v;
    }    
}

let cnn = cnn();

cnn = cnn + input_layer([28, 28])
+ conv_layer(5, 32, 1, 2)
+ pool_layer(2, 2, 0)
+ conv_layer(5, 64, 1, 2)
+ pool_layer(2, 2, 0)
+ full_connected_layer(10)
+ softmax_layer()
;

let ds = sample_dataset(dataset = raw, labels = as.numeric(labels));

cnn = CNN::training(cnn, ds, max.loops = 1, batch.size = 5);


raw[, "label"] = NULL;

let result = CNN::predict(cnn, raw);

result[, "label"] = labels;

print(result);

write.csv(result, file = "./demo-test.csv", row.names = TRUE);

CNN::saveModel(cnn, file = "./MNIST.cnn");

cnn = CNN::cnn(file = "./MNIST.cnn");

let result = CNN::predict(cnn, raw);

result[, "label"] = labels;

print(result);

write.csv(result, file = "./demo-test-reader.csv", row.names = TRUE);