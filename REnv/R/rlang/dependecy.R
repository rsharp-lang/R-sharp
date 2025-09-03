#' An internal helper function for attaching dependency scripts
#' 
#' @description This function processes a collection of R script files or directories,
#'              generating source commands for all identified R script files.
#' 
#' @param source A character vector of file paths or directory paths containing
#'               R scripts to be loaded. If a directory path is provided, all
#'               .R and .r files within it (recursively) will be included.
#' 
#' @return A character string containing source commands for all identified
#'         R script files, or a message indicating no dependencies were required.
#'         The returned string can be evaluated to load all dependency scripts.
#' 
#' @details This function handles both individual R script files and directories.
#'          For directories, it recursively scans for all files with .R or .r extensions.
#'          File paths are normalized to ensure consistency across different platforms.
#' 
const __sourcescript = function(source) {
    let rscripts = [];

    for(let handle in source) {
        if (!file.exists(handle)) {
            # try used as directory path
            # scan all R script file inside this directory folder
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
        # generates the script lines of source("xxx.R");
        paste(`source("${rscripts}");`, sep = "\n");
    }
}