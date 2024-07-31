imports "utils" from "base";

#' Load the internal cancer classify demo dataset
#' 
#' http://www.sph.emory.edu/~dkleinb/datasets/cancer.dat
#' 
const classify_cancer = function() {
    const file as string = system.file("data/cancer.dat", package = "REnv");
    const data = file 
	|> readLines() 
	|> skip(1) 
	|> lapply(function(str) {
        strsplit(str, "\s+", fixed = FALSE, perl = TRUE);
    });
	
    const tags         = sapply(data, x -> x[1]);
    const labels       = sapply(data, x -> x[length(x)]); 
    const i as integer = 2:(length(data[[1]]) - 1); 
    const matrix       = lapply(data, r -> r[i]);
    const dataset      = list();

    for(i in 1:length(i)) {
        dataset[[`V${i}`]] = matrix 
        |> sapply(r -> r[i]) 
        |> as.numeric()
        ;
    }

    let exports = data.frame(dataset);
    exports = cbind(exports, labels = as.numeric(labels));
    rownames(exports) = tags;
    exports;
}

const sacurine = function() {
    utils::readRData(file = system.file("data/sacurine.rds", package = "REnv"));
}

#' ensure the export of this function is a dataframe
#' 
#' @param x the dataframe source, could be:
#' 
#'   1. a file path to the dataframe source, file format could be tsv or csv
#'   2. a dataframe object
#' 
const coerce_dataframe = function(x, row.names = TRUE) {
    if (is.character(x)) {
        read.csv(x, row.names = ifelse(row.names, 1, NULL), check.names = FALSE, 
            tsv = any(file.ext(x) == ["txt" "tsv"]));
    } else {
        if (is.data.frame(x)) {
            x;
        } else {
            stop("the input data x must be a dataframe object!");
        }
    }
}