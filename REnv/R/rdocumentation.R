imports "utils.docs" from "roxygenNet";
imports "rdocumentation" from "roxygenNet";

#' Create html documents for ``R#`` package module
#' 
#' @param pkgName the ``R#`` package module itself 
#'    or the package character text name.
#' @param outputdir the output directory path for 
#'    save the html documents.
#' 
const Rdocuments = function(pkgName, outputdir = "./", package = NULL) {
	const R_syntax_js = getOption("r_syntax.js", default = "../../_assets/R_syntax.js");
    const css_ref = `${dirname(R_syntax_js, fullpath = FALSE)}/page.css`;
    const R_highlights = `${dirname(R_syntax_js, fullpath = FALSE)}/highlights.js`;
	const template as string = "templates/Rdocumentation.html" 
	|> system.file(package = "REnv") 
	|> readText()
	|> gsub("%r_syntax%", R_syntax_js)
    |> gsub("%r_css%", css_ref)
    |> gsub("%r_highlights%", R_highlights)
	;	
	const functions = pkgName |> getFunctions();
	const docs_dir = {
		if (typeof pkgName is "string") {
			pkgName;
		} else {
			[pkgName]::namespace;
		}
	}

    const _css__script_  = system.file("templates/page.css", package = "REnv");
	const syntax_script  = system.file("templates/R_syntax.js", package = "REnv");
    const highlight_func = system.file("templates/highlights.js", package = "REnv");
    
	# 20231215 the previous dir name ".assets" will be ignored by 
	# the github page workflow
	# so change the dir name to current "_assets"
	const vignettes_root = dirname(dirname(outputdir));
	const assets = `${vignettes_root}/_assets/`;

	file.copy(syntax_script, assets);
    file.copy(_css__script_, assets);
    file.copy(highlight_func, assets);

	for(f in names(functions)) {
		functions[[f]]
		|> documentation(template)
		|> writeLines(con = `${outputdir}/${f}.html`)
		;
	}
	
	pkgName
	|> makehtml.docs(package = package)
	|> writeLines(con = `${outputdir}/../${docs_dir}.html`)
	;

	let __clr = rdocumentation::pull_clr_types();

	for(t in __clr) {
		clr_docs(t)
		|> writeLines(con = `${vignettes_root}/clr/${gsub([t]::FullName, ".", "/")}.html`);
	}

	invisible(NULL);
}