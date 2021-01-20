imports "package_utils" from "devkit";

serialize(function(sampleInfo, plot, workdir = "./") {
    let pos = read.csv(`${workdir}/pos/_seq.xls`, row_names = 1, tsv = TRUE);
    let neg = read.csv(`${workdir}/neg/_seq.xls`, row_names = 1, tsv = TRUE);
});