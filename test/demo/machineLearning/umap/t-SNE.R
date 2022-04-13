imports ["dataset", "t-SNE", "clustering"] from "MLkit";

const filename as string = "MNIST-LabelledVectorArray-60000x100.msgpack";
const MNIST_LabelledVectorArray = `${dirname(@script)}/${filename}`
|> read.mnist.labelledvector(takes = 3000)
;
const tags as string = rownames(MNIST_LabelledVectorArray);

rownames(MNIST_LabelledVectorArray) = `X${1:nrow(MNIST_LabelledVectorArray)}`;

bitmap(file = `${dirname(@script)}/MNIST-LabelledVectorArray-20000x100.t-SNE_scatter.png`, size = [6000,4000]) {
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
		
	plot(tSNE,
		clusters    = lapply(tags, t -> t, names = rownames(MNIST_LabelledVectorArray)), 
		labels      = rownames(MNIST_LabelledVectorArray),
		show_bubble = FALSE,
		point_size  = 50,
		legendlabel = "font-style: normal; font-size: 24; font-family: Bookman Old Style;",
		padding     = "padding:150px 150px 350px 350px;"
	);
}