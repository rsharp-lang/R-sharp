#' make native r function call
#' 
#' @param f target r function code to be run in native r environment
#' @param args the function invoke parameters
#' @param workdir the workdir of the sub-process of the native R
#' 
const native_r = function(f, args = list(), 
                        workdir = NULL, 
                        deps = NULL, 
                        debug = FALSE, 
                        requires = NULL,
                        native_R = getOption("native_rexec")) {

    # make translate of the managed R# closure expression
    # to the native R code
    let script_code = f |> transform_rlang_source(
        source = deps, 
        debug = debug
    );    
    let invoke = f |> translate_r_native_call(args);
    let label = attr(invoke, "name");
    let native_rlang = c(script_code, invoke);

    if (length(requires) > 0) {
        requires = sapply(requires, name -> `require(${name});`);
        native_rlang = append(requires, native_rlang);
    }

    native_rlang |> rlang_call(
        workdir = workdir, 
        native_R = native_R,
        title = label
    );
}