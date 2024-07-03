imports "dataset" from "MLkit";
imports "VAE" from "MLkit";

require(Matrix);
require(graphics);

options(SIMD = "legacy");
options(n_threads = 8);

setwd(@dir);

const images_set = "../mnist_dataset/train-images-idx3-ubyte";
const raw = images_set 
|> read.MNIST( 
format = "mnist", 
dataset = "vector", 
labelfile = "../mnist_dataset/train-labels-idx1-ubyte" # , subset = 10000
);

let labels = `N_${names(raw)}`;
let embedding = nmf(raw, rank = 10,  max_iterations = 100);  # VAE::embedding(raw,max_iteration= 2, batch_size = 150);

# str(embedding);

let w = as.data.frame(embedding$W);
rownames(w) = labels;

print(w);

let h = as.data.frame(embedding$H);

rownames(h) = `x${1:ncol(w)}`;

print(h);

write.csv(w, file = "./nmf_class.csv");
write.csv(h, file = "./nmf_deconv.csv");

writeLines(embedding$cost, con = "./nmf_errors.txt");
# write.csv(embedding, file = "./embedding.csv", row.names = TRUE);

