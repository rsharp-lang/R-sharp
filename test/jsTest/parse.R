imports "javascript" from "devkit";

js = javascript::parse(readText(`${@dir}/test1.js`));
js = as.data.frame(js);

print(js);