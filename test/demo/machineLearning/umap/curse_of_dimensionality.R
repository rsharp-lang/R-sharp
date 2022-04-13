imports "dataset" from "MLkit";

options("max.print" = 200);

const fileName = "MNIST-LabelledVectorArray-60000x100.msgpack";
const MNIST_LabelledVectorArray = `${dirname(@script)}/${fileName}`
|> read.mnist.labelledvector()
;

str(MNIST_LabelledVectorArray);

print(rownames(MNIST_LabelledVectorArray));

cat("\n\n");

const n as integer = 500;
const raw = MNIST_LabelledVectorArray[1:n, ];
const euclidean_dimensions as function(raw) {
	str(raw);

	let d = for(i in 15:ncol(raw)) {
		const euclidean = raw[, 1:i]
		|> dist
		|> as.vector
		;		
				
		cat(i);
		cat("\t");
		
		log2((max(euclidean) - min(euclidean)) / (min(euclidean) + 0.00001));
	};
	
	cat("\n\n");
	
	d[2:length(d)] - d[1:(length(d) - 1)];
}

rownames(raw) = `X${1:n}`;

const d = euclidean_dimensions(raw);
const dims = (2:length(d))[d != 0];
const dist = data.frame(dimensions = dims, euclidean = d[d != 0]);

print(d);
print(dist);

write.csv(dist, file = `${dirname(@script)}/curse_of_dimensionality.csv`, row_names = FALSE);