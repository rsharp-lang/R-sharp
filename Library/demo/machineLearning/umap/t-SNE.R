imports ["dataset", "t-SNE", "clustering"] from "MLkit";

const filename as string = "MNIST-LabelledVectorArray-60000x100.msgpack";
const MNIST_LabelledVectorArray = `${dirname(@script)}/${filename}`
|> read.mnist.labelledvector(takes = 200)
;
const tags = rownames(MNIST_LabelledVectorArray);

rownames(MNIST_LabelledVectorArray) = `X${1:nrow(MNIST_LabelledVectorArray)}`;

bitmap(file = `${dirname(@script)}/MNIST-LabelledVectorArray-20000x100.t-SNE_scatter.png`) {
	const tSNE = t.SNE()
	|> data(MNIST_LabelledVectorArray)
	|> solve(iterations = 200)	
	;
	
	tSNE 
	|> as.data.frame 
	|> write.csv(
		file      = `${dirname(@script)}/MNIST-LabelledVectorArray-20000x100.t-SNE_scatter.csv`, 
		row.names = tags
	)
	; 
	
	tSNE
	|> plot(
		clusters = lapply(tags, t -> t, names = rownames(MNIST_LabelledVectorArray)), 
		labels   = rownames(MNIST_LabelledVectorArray)
	)
	;
}