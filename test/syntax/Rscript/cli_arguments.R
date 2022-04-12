# title: demo shell script in R#
#
# author: xieguigang
# 
# description: this is a demo script of show R# shell 
#     scripting annotation features.
#

[@info "The file path of a raw data file."]
let rawfile as string = ?"--input" || stop("missing raw data file input!");

[@info "iterations count."]
[@type "count"]
let iteration as integer = ?"--loops.n" || 1000;

[@info "script is running in debug mode?"]
let flag as boolean = ?"--debug";

print(rawfile);