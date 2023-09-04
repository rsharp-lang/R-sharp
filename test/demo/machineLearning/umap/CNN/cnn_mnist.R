imports ["CNN", "dataset"] from "MLkit";

setwd(@dir);

options(SIMD = "legacy");

const images_set = "../mnist_dataset/train-images-idx3-ubyte";
const raw = images_set 
|> read.MNIST( 
format = "mnist", 
dataset = "dataframe", 
labelfile = "../mnist_dataset/train-labels-idx1-ubyte",
subset = 60
);

# str(raw);

let labels = raw$label;

raw[, "label"] = NULL;

# for(name in colnames(raw)) {
#     let v = as.numeric(raw[, name]);

#     if (sum(v) > 0) {
#         v = v / max(v);
#         raw[, name] = v;
#     }    
# }

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
let cnn_f = CNN::training(cnn, ds, max.loops = 3, trainer = CNN::ada_grad(batch.size = 60));

print(cnn_f);
raw[, "label"] = NULL;

let result = cnn_f(raw);

result[, "label"] = labels;

print(result);

write.csv(result, file = "./demo-test.csv", row.names = TRUE);

CNN::saveModel(cnn_f, file = "./MNIST.cnn");

cnn_f = CNN::cnn(file = "./MNIST.cnn");

let result = CNN::predict(cnn_f, raw);

result[, "label"] = labels;

print(result);

write.csv(result, file = "./demo-test-reader.csv", row.names = TRUE);