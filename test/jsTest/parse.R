imports "javascript" from "devkit";

js = javascript::parse(readText(`${@dir}/test3.js`));
js = as.data.frame(js);

print(js);