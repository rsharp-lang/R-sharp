#' An internal helper function for generates html document for R# runtime symbols
#' 
#' @param symbols a set of R# runtime symbols that defined from the R source script. 
#'    element data type of this parameter could be function, lambda function, character
#'    symbols or something else which has symbol names. 
#' 
#' @param outputdir the directory path for export the help document for the R# runtime
#'    symbols usually be the location of ``{package_dir}/vignettes/R``.
#' 
const __RSymbolDocumentation = function(symbols, package, outputdir = "./") {
	let _template_html = __template()$template;
	let name as string = NULL;

	for(func in symbols) {
		name <- [func]::symbol_name;
		
		# export html documents
		func 
		|> rdocumentation::documentation(_template_html, desc = package)
		|> writeLines(con = `${outputdir}/docs/${name}.html`)
		;
	}

	invisible(NULL);
}