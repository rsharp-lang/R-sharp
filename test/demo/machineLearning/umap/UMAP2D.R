imports ["dataset", "umap"] from "MLkit";

setwd(@dir);

const filename as string = "MNIST-LabelledVectorArray-60000x100.msgpack";
const MNIST_LabelledVectorArray = `./${filename}`
|> read.mnist.labelledvector(takes = 50000)
;
const tags as string = rownames(MNIST_LabelledVectorArray);
const kdtree_metric as boolean = FALSE;

rownames(MNIST_LabelledVectorArray) = `X${1:nrow(MNIST_LabelledVectorArray)}`;

bitmap(file = `./MNIST-LabelledVectorArray-50000x100.umap_scatter${ifelse(kdtree_metric, "_kdtree_KNN", "")}.png`, size = [6000,4000]) {
	const t1 = now();
	const manifold = umap(MNIST_LabelledVectorArray,
		dimension         = 2, 
		numberOfNeighbors = 60,
		localConnectivity = 1,
		KnnIter           = 64,
		bandwidth         = 1,
		debug             = TRUE,
		KDsearch          = kdtree_metric
	)
	;
	const t2 = now();
	
	print(t2 - t1);
	
	manifold$umap
	|> as.data.frame
	|> write.csv( 
		file      = `./MNIST-LabelledVectorArray-50000x100.umap_scatter${ifelse(kdtree_metric, "_kdtree_KNN", "")}.csv`, 
		row.names = tags
	);
	
	plot(manifold$umap,
		clusters    = lapply(tags, t -> t, names = rownames(MNIST_LabelledVectorArray)), 
		labels      = rownames(MNIST_LabelledVectorArray),
		show_bubble = FALSE,
		point_size  = 50,
		legendlabel = "font-style: normal; font-size: 24; font-family: Bookman Old Style;",
		padding     = "padding:150px 150px 350px 350px;"
	);
}
