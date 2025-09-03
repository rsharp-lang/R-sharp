#' translate R# code to Rscript text
#' 
#' @param code the R# managed closure expression code for translated to the native R code in this function
#' @param source the external dependency native rscript file path
#' 
#' @return this function build rscript text from a given R# closure code
#' @details this function could be used for generates script for run in docker container
#' 
const transform_rlang_source = function(code, source = NULL, debug = FALSE) {
    let load_deps = __sourcescript(source);
    # make translate of the managed R# closure expression
    # to the native R code
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