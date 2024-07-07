#' helper function for R# interop with R language
#' 
#' @param code a closure expresion for invoke R function
#' @param source the source file paths for provides the 
#'    runtime environment for the specific input code. 
#' 
const rlang_interop = function(code, source = NULL, debug = FALSE) {
    let code_save = tempfile(fileext = ".R");
    let load_deps = __sourcescript(source);
    let program   = .Internal::translate_to_rlang(code);

    code <- paste([load_deps, program], sep = "\n\n");

    if (debug) {
        cat(code);
        cat("\n");
    } else {
        writeLines(code, con = code_save);
        system(`Rscript "${code_save}"`);
    }
}

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