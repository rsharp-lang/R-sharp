
#' An internal helper function for try to get the html document template
#' 
#' @return a tuple list that contains two template elements:
#' 
#'   + template: the html document template for the R# runtime symbols which is defined inside the R source script files.
#'   + clr_template: the html document template for the clr object which is exported from the .NET dll assembly files.
#' 
const __template = function() {
	let R_syntax_js = getOption("r_syntax.js", default = "../../_assets/R_syntax.js");
    let css_ref = `${dirname(R_syntax_js, fullpath = FALSE)}/page.css`;
    let R_highlights = `${dirname(R_syntax_js, fullpath = FALSE)}/highlights.js`;

	let doc_template = getOption("doc_template");
	let clr_template = getOption("clr_template");

	let template = ifelse(nchar(doc_template) == 0, 
		system.file("templates/Rdocumentation.html", package = "REnv"), 
		doc_template
	)
	|> readText()
	|> gsub("%r_syntax%", R_syntax_js)
    |> gsub("%r_css%", css_ref)
    |> gsub("%r_highlights%", R_highlights)
	;	

	let clr_template = ifelse(nchar(clr_template) == 0, 
		system.file("templates/clr_template.html", package = "REnv"), 
		clr_template
	) 
	|> readText()
	|> gsub("%r_syntax%", R_syntax_js)
    |> gsub("%r_css%", css_ref)
    |> gsub("%r_highlights%", R_highlights)
	;	

	list(template, clr_template);
}