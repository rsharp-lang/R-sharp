imports "javascript" from "devkit";

js = javascript::parse(readText(`${@dir}/test3.js`));
js = as.data.frame(js);
js = invoke_parameters([js$expr_raw]::obj);
js = as.data.frame([js]::body);

print(js);

js = js[js$clr_type == "DeclareNewSymbol", ];

for(symbol in js$expr_raw) {
	v = symbol_value(unlist(symbol));
	v = eval(v);
	
	str(v);
	cat("\n\n\n");
}

print(js);