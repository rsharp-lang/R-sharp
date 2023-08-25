imports ["CNN", "dataset"] from "MLkit";

setwd(@dir);

options(SIMD = "legacy");

const images_set = "../mnist_dataset/train-images-idx3-ubyte";
const raw = images_set 
|> read.MNIST( 
format = "mnist", 
dataset = "dataframe", 
labelfile = "../mnist_dataset/train-labels-idx1-ubyte",
subset = 12001
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

let cnn = CNN::cnn(file = "./MNIST.cnn");
let result = CNN::predict(cnn, raw);

result[, "label"] = labels;

print(result);

write.csv(result, file = "./demo-test-reader.csv", row.names = TRUE);