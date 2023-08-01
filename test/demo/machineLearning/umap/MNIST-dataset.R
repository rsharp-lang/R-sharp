imports "dataset" from "MLkit";

let raw = read.MNIST.labelledvector(`${@dir}/MNIST-LabelledVectorArray-60000x100.msgpack`, takes = 10);

print(raw);