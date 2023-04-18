imports "javascript" from "devkit";

js = javascript::parse(readText(`${@dir}/test2.js`));
js = as.data.frame(js);

print(js);