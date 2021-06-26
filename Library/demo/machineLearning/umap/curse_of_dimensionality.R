imports "dataset" from "MLkit";

options("max.print" = 200);

const fileName = "MNIST-LabelledVectorArray-60000x100.msgpack";
const MNIST_LabelledVectorArray = `${dirname(@script)}/${fileName}`
|> read.mnist.labelledvector()
;

str(MNIST_LabelledVectorArray);

print(rownames(MNIST_LabelledVectorArray));

const n as integer = 300;
const raw = MNIST_LabelledVectorArray[1:n, ];

rownames(raw) = `X${1:n}`;

const d = for(i in 15:ncol(MNIST_LabelledVectorArray)) {
	let euclidean = raw[, 1:i]
	|> dist
	|> as.vector
	;
		
	print(`dimension of ${i}:`);
	# print(euclidean);
	
	# cat("\n");
	
	log10((max(euclidean) - min(euclidean)) / (min(euclidean) + 0.00001));
};

print(d);