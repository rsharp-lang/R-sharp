from MLkit import dataset
from MLkit import umap

setwd(@dir)

filename = "MNIST-LabelledVectorArray-60000x100.msgpack"
MNIST_LabelledVectorArray = `../${filename}`.read.mnist.labelledvector(takes = 50000)
tags = rownames(MNIST_LabelledVectorArray)
rownames(MNIST_LabelledVectorArray) = `X${1:nrow(MNIST_LabelledVectorArray)}`

bmp2 = "./MNIST-LabelledVectorArray-20000x100.umap_scatter2d.png"
bmp3 = "./MNIST-LabelledVectorArray-20000x100.umap_scatter3d.png"

manifold = umap(MNIST_LabelledVectorArray, dimension = 3, debug = TRUE)
umap3 = as.data.frame(manifold$umap, row.names = tags)
umap3_file = "./MNIST-LabelledVectorArray-20000x100.umap_scatter3.csv"

write.csv(umap3, file = umap3_file)

manifold = umap(umap3, dimension = 2, debug = TRUE)

umap2 = as.data.frame(manifold$umap, row.names = tags)
umap2_file = "./MNIST-LabelledVectorArray-20000x100.umap_scatter2.csv"

write.csv(umap2, file = umap2_file)
