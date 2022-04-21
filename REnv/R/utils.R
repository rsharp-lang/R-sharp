imports ["Html", "http", "graphquery"] from "webKit";

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