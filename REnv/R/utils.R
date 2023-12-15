imports ["Html", "http", "graphquery"] from "webKit";
imports "utils.docs" from "roxygenNet";
imports "rdocumentation" from "roxygenNet";

#' Run graphquery on html document
#'
#' @param url the url or local filepath of the target html document
#' @param graphquery the script text of a required given graphquery
#'
#' @return A ``R#`` object that parsed from the target html web
#'    page with given query expression value.
#'
const queryWeb = function(url, graphquery) {
    const query = graphquery::parseQuery(graphquery);

    getHtml(url)
    |> Html::parse
    |> graphquery::query(query)
    ;
}

#' get system os name
#' 
#' @return "Windows" for Microsoft Windows
#'
const platformName = function() {
	const os = Sys.info();
	const name = os$"sysname";
	
	if (name == "Win32NT") {
		"Windows";
	} else {
		name;
	}	
}

#' Create html documents for ``R#`` package module
#' 
#' @param pkgName the ``R#`` package module itself 
#'    or the package character text name.
#' @param outputdir the output directory path for 
#'    save the html documents.
#' 
const Rdocuments = function(pkgName, outputdir = "./", package = NULL) {
	const template as string = "templates/Rdocumentation.html" 
	|> system.file(package = "REnv") 
	|> readText()
	;
	const functions = pkgName |> getFunctions();
	const docs_dir = {
		if (typeof pkgName is "string") {
			pkgName;
		} else {
			[pkgName]::namespace;
		}
	}
	const syntax_script = system.file("templates/R_syntax.js", package = "REnv");
	const assets = `${dirname(dirname(outputdir))}/.assets/`;

	file.copy(syntax_script, assets);

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
}