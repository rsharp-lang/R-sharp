require(JSON);

imports ["dataset", "CNN"] from "MLkit";

setwd(@dir);

let labels = "./nist_dataset/index.json"
|> readText()
|> JSON::json_decode()
;
let matrixSet = lapply(names(labels), function(path) {
    readImage(path);
});

names(matrixSet) = names(labels);

str(labels);

let ds = sample_dataset.image(matrixSet, labels = labels);
let dataset = as.MLdataset(ds);

write.ML_model(dataset, file = "./images_x4.pack");