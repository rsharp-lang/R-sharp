#' Execute native R script from current R# process
#' 
#' @description This function executes a native R script file using the system's Rscript executable.
#' 
#' @param script_code File path to the target native R script file to be executed.
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
const rlang_call = function(script_code, workdir = NULL, native_R = getOption("native_rexec")) {
    let code_save = tempfile(fileext = ".R");
    let current_wd = getwd();
    let change_wd = nchar(workdir) > 0;
    
    writeLines(script_code, con = code_save);

    if (change_wd) {
        dir.create(workdir);
        setwd(workdir);
    }       

    if (nchar(native_R) == 0) {
        if ((Sys.info()[['sysname']]) != "Win32NT") {
            # ubuntu linux
            # system2("/usr/lib/R/bin/Rscript", code_save);
            native_R <- "/usr/lib/R/bin/Rscript";
        } else {
            # window environment
            # system2("C:\\Program Files\\R\\R-4.5.0\\bin\\x64\\Rscript.exe", code_save);
            native_R <- "C:\\Program Files\\R\\R-4.5.0\\bin\\x64\\Rscript.exe";
        }     
    }

    print("# run native R script:");
    message(`${getwd()}$ "${native_R}" "${code_save}"`);
    # call
    system2(native_R, code_save);

    if (change_wd) {
        setwd(current_wd);
    }

    print("end of rlang_interop~~~");
}