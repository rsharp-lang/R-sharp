imports ["dataset", "t.SNE"] from "MLkit";

const MNIST_LabelledVectorArray = `${dirname(@script)}/MNIST-LabelledVectorArray-60000x100.msgpack`
|> read.mnist.labelledvector(takes = 25000)
;

bitmap(file = `${dirname(@script)}/MNIST-LabelledVectorArray-20000x100.t-SNE_scatter.png`) {
	t.SNE::algorithm()
	|> data(MNIST_LabelledVectorArray)
	|> solve(100)
	|> plot
	;
}