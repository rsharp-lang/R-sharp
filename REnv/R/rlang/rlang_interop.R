#' Helper function for R# language interoperability with R language
#' 
#' @description This function facilitates interoperability between R# and native R
#'              by translating R# closure expressions to native R code and executing it.
#' 
#' @param code An R# managed closure expression to be converted and executed as native R code.
#' @param source External dependency script file paths required for the execution environment.
#' @param debug Logical indicating whether to debug the translation process (default: FALSE).
#' @param workdir Working directory for executing the R script (default: current directory).
#' @param print_code Logical indicating whether to print the generated R code (default: FALSE).
#' @param native_R Path to custom Rscript executable (default: from options or system default).
#' 
#' @return The result of executing the translated native R code, typically the output
#'         of the R script execution.
#' 
#' @details This function enables R# code to leverage native R functionality by
#'          translating R# expressions to native R code and executing it through Rscript.
#'          It handles both Windows and Linux environments automatically. 
#' 
const rlang_interop = function(code, 
    source = NULL, 
    debug = FALSE, 
    workdir = NULL, 
    print_code = FALSE, 
    native_R = getOption("native_rexec")) {
    
    # make translate of the managed R# closure expression
    # to the native R code
    let script_code = transform_rlang_source(
        code, source, 
        debug = debug);    

    if (print_code) {
        cat("\n\n");
        print(script_code);
        cat("\n\n");
    }

    if (!debug) {
        script_code |> rlang_call(
            workdir  = workdir, 
            native_R = native_R
        );
    }

    invisible(NULL);
}


