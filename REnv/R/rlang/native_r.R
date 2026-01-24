const native_r = function(f, args = list(), source = NULL, debug = FALSE, native_R = getOption("native_rexec")) {
    # make translate of the managed R# closure expression
    # to the native R code
    let script_code = f |> transform_rlang_source(source, debug = debug);    

    
}