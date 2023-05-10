import {jQuery, Html} from "webKit";

var doc = jQuery.load("./html_demo.html");
var tbl = doc[".stattbl"]
var body = tbl["tbody"]
var rows = body["tr"]
// var first = rows[1]
var taxonomics = lapply(rows, function(r) {
	var cells = r["td"]
	var name = cells[1]
	
	name = name.innerHTML;
	
	var id = Html.link(name);
	
	name = Html.plainText(name);
	
	return {
		id: id, 
		name: name
	}
});

console.log(taxonomics)