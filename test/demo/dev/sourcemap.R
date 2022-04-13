# test sourcemap

imports "VisualStudio" from "devkit";

print(sourceMap(readText(`${!script$dir}/evaluate.json`)));