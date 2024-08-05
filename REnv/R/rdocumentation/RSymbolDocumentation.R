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