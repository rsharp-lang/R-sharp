imports "javascript" from "devkit";

js = javascript::parse(readText(`${@dir}/test3.js`));
js = as.data.frame(js);
js = invoke_parameters([js$expr_raw]::obj);
js = as.data.frame([js]::body);

print(js);