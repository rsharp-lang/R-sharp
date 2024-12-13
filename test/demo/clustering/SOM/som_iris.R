imports "clustering" from "MLkit";

setwd(@dir);

data(bezdekIris);
print(bezdekIris);

let class_labels = bezdekIris$class;

bezdekIris <- bezdekIris[, c("D1","D2","D3","D4")];

let model = somgrid(xdim = 10, ydim = 5);
let result = som(bezdekIris, grid=model);
let class_id = [result]::class_id;

print(class_id);
print(class_labels);

str(as.list(class_id, names = class_labels));

# svg(file = "bezdekIris_SOM.svg", size = [3600,2400]) {
#     plot(result, point.size = 32);
# }