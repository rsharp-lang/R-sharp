imports "automation" from "devkit";

[@config "/dir"]
const dir as string = ?"--dir" || stop("no input data!");

[@config "/image/format"]
const format as string   = ?"--format"   || "png";
[@config "/image/ppi"]
const ppi as integer     = ?"--ppi"      || 300;
[@config "/image/filename"]
const filename as string = ?"--filename" || "test.png";

print(`data input dir: ${dir}`);
print(`image output: '${dir}/${filename}' in format of ${format} and image resolution ppi value is '${ppi}'`);

pause();