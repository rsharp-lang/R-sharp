require(roxygen);

let test_script = "G:\GCModeller\src\workbench\pkg\R\enrichment\kegg_report\union_render.R";
let docs = roxygen::parse(test_script);

for(let name in names(docs)) {
    print(`view comment docs for function: ${name}:`);
    str(docs[[name]]);
}
