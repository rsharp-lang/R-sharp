#' An internal helper function for attach the dependency scripts
#' 
#' @param source a collection of the source scripts filepath for 
#'    load inside the context.
#' 
const __sourcescript = function(source) {
    let rscripts = [];

    for(let handle in source) {
        if (!file.exists(handle)) {
            handle <- list.files(handle, pattern = ["*.R" "*.r"], 
                recursive = TRUE);
        } else {
            handle <- normalizePath(handle);
        }

        rscripts <- append(rscripts, handle);
    }

    if (length(rscripts) == 0) {
        "# no dependecy rscript was required!";
    } else {
        paste(`source("${rscripts}");`, sep = "\n");
    }
}