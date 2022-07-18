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

const Rdocuments = function(pkgName, outputdir = "./") {
	const template as string = "templates/Rdocumentation.html" 
	|> system.file(package = "REnv") 
	|> readText()
	;
	const functions = pkgName |> getFunctions();

	for(f in names(functions)) {
		functions[[f]]
		|> documentation(template)
		|> writeLines(con = `${outputdir}/${pkgName}/${f}.html`)
		;
	}
	
	pkgName
	|> makehtml.docs()
	|> writeLines(con = `${outputdir}/${pkgName}/index.html`)
	;
}