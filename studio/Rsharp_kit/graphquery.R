imports ["Html", "http", "graphquery"] from "webKit";

require(JSON, quietly = TRUE);

# title: graphquery commandline tool
# author: xieguigang
# description: run graphquery on commandline

[@info "the html data page its url reference."]
[@type "file|url"]
const url as string = ?"--url" || stop("no data source is provided!");

[@info "the file path of the graphquery file."]
[@type "file"]
const query as string = ?"--query" || stop("a graph query script must provided!");

[@info "the json file path for save the query result. if this argument is omit, then jsoncontent will be printed on standard output."]
[@type "file"]
const savefile as string = ?"--out";

writeLines(con = savefile) {
	url
	:> requests.get
	:> content
	:> Html::parse 
	:> graphquery::query(graphquery::parseQuery(readText(query)), raw = TRUE)
	:> json_encode
	;
}