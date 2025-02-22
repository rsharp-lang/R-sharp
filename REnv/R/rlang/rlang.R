#' translate R# code to Rscript text
#' 
#' @return this function build rscript text from a given R# closure code
#' @details this function could be used for generates script for run in docker container
#' 
const transform_rlang_source = function(code, source = NULL, debug = FALSE) {
    let load_deps = __sourcescript(source);
    let program   = translate_to_rlang(code);
    let script_code <- paste([
        "sink(file = 'rlang_interop.log', split = TRUE);",
        load_deps, 
        "# --------end of load deps----------", 
        program,
        "sink();"
    ], sep = "\n\n");

    if (debug) {
        print("translate R# code to rlang script:");
        cat(script_code);
        cat("\n\n");
    }

    script_code;
} 

#' helper function for R# interop with R language
#' 
#' @param code a closure expresion for invoke R function
#' @param source the source file paths for provides the 
#'    runtime environment for the specific input code. 
#' 
const rlang_interop = function(code, 
    source = NULL, 
    debug = FALSE, 
    workdir = NULL, 
    print_code = FALSE) {

    let code_save = tempfile(fileext = ".R");
    let script_code = transform_rlang_source(code, source, 
            debug = debug);

    if (!debug) {
        let current_wd = getwd();
        let change_wd = nchar(workdir) > 0;
        
        if (change_wd) {
            setwd(workdir);
        }
        if (print_code) {
            cat("\n\n");
            print(script_code);
            cat("\n\n");
        }

        print("run script at workspace:");
        print(getwd());
        print(code_save);
        print(`Rscript "${code_save}"`);

        writeLines(script_code, con = code_save);
        system(`/usr/lib/R/bin/Rscript "${code_save}"`);

        if (change_wd) {
            setwd(current_wd);
        }

        print("end of rlang_interop~~~");
    }
}

