# load R# library for run dbscan
from MLkit import clustering 
# load R# library for plots
import graphics2D

setwd(@dir)

multishapes = read.csv("./multishapes.csv")
x, y        = (multishapes[, "x"], multishapes[, "y"])

print(`contains ${x.length()} data points.`)
print(multishapes, max.print = 13)

# detect object from umap data
objects = `object_${graphics2D::pointVector(x, y).dbscan_objects()}`
# show object detection result
margin  = "padding: 125px 300px 200px 200px;"
plt     = plot(x, y, class = objects, grid.fill = "white", padding = margin)

bitmap(plt, file = "./object_detection_py.png")