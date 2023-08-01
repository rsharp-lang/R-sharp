imports "dataset" from "MLkit";

let raw = read.MNIST.labelledvector(`${@dir}/MNIST-LabelledVectorArray-60000x100.msgpack`);

raw[, "num"] = as.numeric(rownames(raw));

rownames(raw) = make.names(`num_${rownames(raw)}`, unique = TRUE);

print(raw);

raw = toFeatureSet(raw);
raw = as.MLdataset(raw, labels = "num");

write.ML_model(raw, file = `${@dir}/MNIST-LabelledVectorArray-60000x100.pack`);