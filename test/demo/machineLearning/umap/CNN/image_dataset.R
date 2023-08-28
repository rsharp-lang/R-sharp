imports ["CNN", "dataset"] from "MLkit";
imports "Matrix" from "Rlapack";

require(graphics);

setwd(@dir);

const unit_size = 28;
const x4_size = [unit_size, unit_size] * 2;

let populate_dataset = function(n) {
    const images_set = "../mnist_dataset/train-images-idx3-ubyte";
    const raw = images_set 
    |> read.MNIST( 
        format = "mnist", 
        dataset = "dataframe", 
        labelfile = "../mnist_dataset/train-labels-idx1-ubyte",
        subset = 4 * n
    );

    let labels = raw$label;
    let ds = list();

    raw[, "label"] = NULL;

    for(name in colnames(raw)) {
        let v = as.numeric(raw[, name]);

        if (sum(v) > 0) {
            v = v / max(v);
            raw[, name] = v;
        }    
    }

    print(labels);

    let ranges = split(1:nrow(raw),size = 4);
    let i = 0;

    for(rng in ranges) {
        let label_subset = labels[rng];
        let matrix_set = lapply(rng, function(i) {
            matrix(as.numeric(unlist(raw[i,,drop = TRUE])), nrow = 28, byrow = TRUE);
        });       
        let filename = `${paste(rng, sep = "")}-${i = i + 1}`;

        ds[[filename]] = list(
            labels = label_subset,
            matrix_set = matrix_set,
            filename = filename
        );
    }

    ds;
}

let i = 0;
let ds_labels = list();

for(v in populate_dataset(6)) {
    let labelfile = `./nist_dataset/${v$filename}.png`;
    let labels = v$labels;
    let matrix_set = v$matrix_set;

    ds_labels[[labelfile]] = labels;

    bitmap(file = labelfile, size = x4_size) {
        
    }
}

require(JSON);

ds_labels
|> JSON::json_encode()
|> writeLines(
    con = "./nist_dataset/index.json"
);
