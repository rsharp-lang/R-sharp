#' helper function for R# language interop with R language
#' 
#' @param code a R# managed closure expresion code for invoke native R function
#' @param source the source file paths for provides the 
#'    runtime environment for the specific input code. 
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
}

#' make native R script call from current R# process
#' 
#' @param code_save the file path to the target native rscript file
#' @param workdir the workspace directory path for run the native rscript file. leaves blank means use current workdir.
#' @param native_R the custom Rscript program location for run the native rscripr file
#' 
#' @return this function returns nothing
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
