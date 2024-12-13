imports "clustering" from "MLkit";

setwd(@dir);

data(bezdekIris);
print(bezdekIris);

let class_labels = bezdekIris$class;

bezdekIris <- bezdekIris[, c("D1","D2","D3","D4")];

let model = somgrid(xdim = nrow(bezdekIris), ydim = 2);
let result = som(bezdekIris, grid=model);
let class_id = [result]::class_id;

print(class_id);
print(class_labels);

svg(file = "bezdekIris_SOM.svg") {
    plot(result);
}