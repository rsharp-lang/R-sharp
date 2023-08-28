imports ["CNN", "dataset"] from "MLkit";
imports "Matrix" from "Rlapack";

require(graphics);

setwd(@dir);

options(SIMD = "legacy");

const images_set = "../mnist_dataset/train-images-idx3-ubyte";
const raw = images_set 
|> read.MNIST( 
format = "mnist", 
dataset = "dataframe", 
labelfile = "../mnist_dataset/train-labels-idx1-ubyte",
subset = 50
);

# str(raw);

let labels = raw$label;

raw[, "label"] = NULL;

for(name in colnames(raw)) {
    let v = as.numeric(raw[, name]);

    if (sum(v) > 0) {
        v = v / max(v);
        raw[, name] = v;
    }    
}

print(labels);
# str(raw);

let a = matrix(as.numeric(unlist(raw[1,, drop = TRUE ])), nrow = 28, byrow = TRUE);
let b = matrix(as.numeric(unlist(raw[2,, drop = TRUE ])), nrow = 28, byrow = TRUE);
let c = matrix(as.numeric(unlist(raw[3, drop = TRUE ])), nrow = 28, byrow = TRUE);
let d = matrix(as.numeric(unlist(raw[4,, drop = TRUE ])), nrow = 28, byrow = TRUE);

print(as.numeric(unlist(raw[1,, drop = TRUE ])));
print(a);

let abcd = labels[1:4];
let i = 0;

for(let matrix of [a, b ,c, d]) {
    bitmap(file = `./nist_images/${abcd[i = i + 1]}.png`) {
        image(matrix);
    }
}

