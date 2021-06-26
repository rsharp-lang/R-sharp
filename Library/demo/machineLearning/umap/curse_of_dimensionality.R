imports "dataset" from "MLkit";

options("max.print" = 200);

const fileName = "MNIST-LabelledVectorArray-60000x100.msgpack";
const MNIST_LabelledVectorArray = `${dirname(@script)}/${fileName}`
|> read.mnist.labelledvector()
;

str(MNIST_LabelledVectorArray);

print(rownames(MNIST_LabelledVectorArray));

const euclidean_dimensions as function(raw) {
	str(raw);

	for(i in 2:ncol(raw)) {
		let euclidean = raw[, 1:i]
		|> dist
		|> as.vector
		;
			
		print(`dimension of ${i}:`);
		# print(euclidean);
		
		# cat("\n");
		
		log10((max(euclidean) - min(euclidean)) / (min(euclidean) + 0.00001));
	};
}

const sparse = euclidean_dimensions(raw = dataset::gaussian(size = 100, dimensions = 100, pzero = 0.85, nclass = 10));

print(sparse);