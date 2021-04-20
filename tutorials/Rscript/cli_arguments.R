
[@info "The file path of a raw data file."]
let rawfile as string = ?"--input" || stop("missing raw data file input!");

[@info "iterations count."]
let iteration as integer = ?"--loops.n" || 1000;

print(rawfile);