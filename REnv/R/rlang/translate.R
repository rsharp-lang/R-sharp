#' Translate R# code to native R script text
#' 
#' @description This function converts R# managed closure expressions to executable native R code.
#' 
#' @param code The R# managed closure expression code to be translated to native R code.
#' @param source External dependency native R script file paths required for the translation.
#' @param debug Logical indicating whether to debug the translation process (default: FALSE).
#' 
#' @return A character string containing the complete native R script code that can be executed
#'         by an R interpreter, including all dependency loading and the translated code.
#' 
#' @details This function is particularly useful for generating R scripts that can be executed
#'          in environments like Docker containers or other isolated systems where R# might
#'          not be available. The translation process preserves the functionality of the
#'          original R# code while making it compatible with standard R interpreters. [9](@ref)
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