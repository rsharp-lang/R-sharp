import "jQuery" from "webKit";

var doc = jQuery.load("./html_demo.html");
var tbl = doc[".stattbl"]
var body = tbl["tbody"]
var rows = body["tr"]
// var first = rows[1]
var taxonomics = lapply(rows, function(r) {
	var cells = r["td"]
	var name = cells[1]
	
	return name.innerHTML;
});

console.log(taxonomics)