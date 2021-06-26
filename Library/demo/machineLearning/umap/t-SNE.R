imports ["dataset", "t-SNE", "clustering"] from "MLkit";

const MNIST_LabelledVectorArray = `${dirname(@script)}/MNIST-LabelledVectorArray-60000x100.msgpack`
|> read.mnist.labelledvector(takes = 2000)
;
const tags = rownames(MNIST_LabelledVectorArray);

rownames(MNIST_LabelledVectorArray) = `X${1:nrow(MNIST_LabelledVectorArray)}`;

bitmap(file = `${dirname(@script)}/MNIST-LabelledVectorArray-20000x100.t-SNE_scatter.png`) {
	tSNE_algorithm()
	|> data(MNIST_LabelledVectorArray)
	|> solve(200)
	|> plot(
		clusters = lapply(tags, t -> t, names = rownames(MNIST_LabelledVectorArray)), 
		labels   = rownames(MNIST_LabelledVectorArray)
	)
	;
}