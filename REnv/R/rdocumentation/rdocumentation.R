imports "utils.docs" from "roxygenNet";
imports "rdocumentation" from "roxygenNet";

#' Create html documents for ``R#`` package module
#' 
#' @param pkgName the ``R#`` package module itself 
#'    or the package character text name.
#' @param outputdir the output directory path for 
#'    save the html documents.
#' @param package a character vector for display the package name
#' 
#' @details the parameter pkgName is different with package parameter, for
#' 
#' 1. package parameter is standard for the R# zip package its namespace
#' 2. pkgName parameter usually be one of a clr dll module
#' 
const Rdocuments = function(pkgName, outputdir = "./", package = NULL) {
	const [template, clr_template] = __template();
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

	let __clr = rdocumentation::pull_clr_types(generic.excludes = TRUE);

	# visit the clr type reference tree
	for(i in 1:1000) {
		if (length(__clr) > 0) {
			for(t in __clr) {
				clr_docs(t, clr_template)
				|> writeLines(con = `${vignettes_root}/clr/${gsub([t]::FullName, ".", "/")}.html`);
			}

			__clr = rdocumentation::pull_clr_types(generic.excludes = TRUE);
		} else {
			break;
		}
	}

	invisible(NULL);
}