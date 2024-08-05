
#' try to get the html document template
#' 
const __template = function() {
	const R_syntax_js = getOption("r_syntax.js", default = "../../_assets/R_syntax.js");
    const css_ref = `${dirname(R_syntax_js, fullpath = FALSE)}/page.css`;
    const R_highlights = `${dirname(R_syntax_js, fullpath = FALSE)}/highlights.js`;

	const template = "templates/Rdocumentation.html" 
	|> system.file(package = "REnv") 
	|> readText()
	|> gsub("%r_syntax%", R_syntax_js)
    |> gsub("%r_css%", css_ref)
    |> gsub("%r_highlights%", R_highlights)
	;	

	const clr_template = "templates/clr_template.html"
	|> system.file(package = "REnv") 
	|> readText()
	|> gsub("%r_syntax%", R_syntax_js)
    |> gsub("%r_css%", css_ref)
    |> gsub("%r_highlights%", R_highlights)
	;	

	list(template, clr_template);
}