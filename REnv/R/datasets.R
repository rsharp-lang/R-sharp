
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
	
	str(data);
	
    const tags = sapply(data, x -> x[1]);str(tags);
    const labels = sapply(data, x -> x[length(x)]); str(labels);
    const i as integer = 2:(length(data) - 1); str(i);
    const matrix = lapply(data, r -> i[i]);
    const dataset = list();
stop();
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