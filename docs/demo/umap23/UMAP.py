from MLkit import dataset
from MLkit import umap

setwd(@dir)

MNIST_LabelledVectorArray = "../../../test/demo/machineLearning/umap/MNIST-LabelledVectorArray-60000x100.msgpack"
MNIST_LabelledVectorArray = MNIST_LabelledVectorArray.read.mnist.labelledvector(takes = 10000)

tags                                = `class_${rownames(MNIST_LabelledVectorArray)}`
rownames(MNIST_LabelledVectorArray) = `X${1:nrow(MNIST_LabelledVectorArray)}`

print("previews of the number tags:")
print(tags, max.print = 100)

bmp2       = "./MNIST-LabelledVectorArray-20000x100.umap_scatter2d.png"
bmp3       = "./MNIST-LabelledVectorArray-20000x100.umap_scatter3d.png"
umap3_file = "./MNIST-LabelledVectorArray-20000x100.umap_scatter3.csv"
umap2_file = "./MNIST-LabelledVectorArray-20000x100.umap_scatter2.csv"

# umap 3d scatter
manifold        = umap(MNIST_LabelledVectorArray, dimension = 3, debug = True)
umap3           = as.data.frame(manifold$umap)
rownames(umap3) = tags

print("previews of the umap 3d data:")
print(umap3, max.print = 6)

# save data matrix
write.csv(umap3, file = umap3_file)

# continue to 2d scatter
manifold        = umap(umap3, dimension = 2, debug = True)
umap2           = as.data.frame(manifold$umap)
rownames(umap2) = tags

print("previews of the umap 2d data:")
print(umap2, max.print = 6)

# save data matrix
write.csv(umap2, file = umap2_file)

