require(REnv);

let deps = [
    "G:\GCModeller\src\R-sharp\REnv\R\vbcode_banner.R"
    "G:\GCModeller\src\R-sharp\REnv\R\zzz.R"
    "G:\GCModeller\src\R-sharp\REnv\R\http.R"
    "G:\GCModeller\src\R-sharp\REnv\R\rdocumentation.R"
    "G:\GCModeller\src\R-sharp\REnv\R\utils.R"
];
let data_file = "a.csv";
let rscript = ~{
    let a = read.csv(data_file, row.names = 1, check.names = FALSE);

    print(a);

    let result_pca = prcomp(a);

    summary(result_pca);

}

rlang_interop(rscript, source = deps, debug = TRUE);