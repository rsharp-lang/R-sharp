imports ["CNN", "dataset"] from "MLkit";

setwd(@dir);

options(SIMD = "legacy");

const images_set = "../mnist_dataset/train-images-idx3-ubyte";
const raw = images_set 
|> read.MNIST( 
format = "mnist", 
dataset = "dataframe", 
labelfile = "../mnist_dataset/train-labels-idx1-ubyte",
subset = 25001
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
+ conv_layer(6, [5, 5])
+ pool_layer([2, 2])
+ conv_layer(12, [5, 5])
+ pool_layer([2, 2])
+ output_layer(class.num = 10)
;

cnn = CNN::training(cnn, dataset = raw, labels = as.numeric(labels), max.loops = 6, batch.size = 50);


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