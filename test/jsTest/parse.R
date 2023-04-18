imports "javascript" from "devkit";

require(JSON);

js = javascript::parse(readText(`${@dir}/test3.js`));
js = as.data.frame(js);
js = invoke_parameters([js$expr_raw]::obj);
js = as.data.frame([js]::body);

print(js);

js = js[js$clr_type == "DeclareNewSymbol", ];

for(symbol in js$expr_raw) {
	v = symbol_value(unlist(symbol));
	v = eval(v);
	
	name = symbol_name(unlist(symbol));
	
	str(v);
	cat("\n\n\n");
	
	v 
	|> JSON::json_encode()
	|> writeLines(
		con = `${@dir}/${name}.json`
	);
}

print(js);