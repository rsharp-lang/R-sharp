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

    let code_save = tempfile(fileext = ".R");
    # make translate of the managed R# closure expression
    # to the native R code
    let script_code = transform_rlang_source(
        code, source, 
        debug = debug);

    writeLines(script_code, con = code_save);

    if (print_code) {
        cat("\n\n");
        print(script_code);
        cat("\n\n");
    }

    if (!debug) {
        code_save |> rlang_call(
            workdir = workdir, 
            native_R = native_R
        );
    }

    invisible(NULL);
}

#' Execute native R script from current R# process
#' 
#' @description This function executes a native R script file using the system's Rscript executable.
#' 
#' @param code_save File path to the target native R script file to be executed.
#' @param workdir Working directory for executing the R script (default: current directory).
#' @param native_R Path to custom Rscript executable (default: from options or system default).
#' 
#' @return This function returns nothing explicitly, but executes the R script and
#'         may produce output, side effects, or results based on the script's contents.
#' 
#' @details The function automatically detects the operating system and uses the
#'          appropriate Rscript executable path for Windows or Linux systems.
#'          It temporarily changes the working directory if specified, then restores
#'          the original directory after execution. 
#'  
const rlang_call = function(code_save, workdir = NULL, native_R = getOption("native_rexec")) {
    let current_wd = getwd();
    let change_wd = nchar(workdir) > 0;
    
    if (change_wd) {
        setwd(workdir);
    }

    print("run script at workspace:");
    print(getwd());
    print(code_save);
    print(`Rscript "${code_save}"`);        

    if (nchar(native_R) == 0) {
        if ((Sys.info()[['sysname']]) != "Win32NT") {
            # ubuntu linux
            system2("/usr/lib/R/bin/Rscript", code_save);
        } else {
            # window environment
            system2("C:\\Program Files\\R\\R-4.5.0\\bin\\x64\\Rscript.exe", code_save);
        } 
    } else {
        system2(native_R, code_save);
    }

    if (change_wd) {
        setwd(current_wd);
    }

    print("end of rlang_interop~~~");
}
