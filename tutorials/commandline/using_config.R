imports "automation" from "devkit";

[@config "/dir"]
const dir as string = ?"--dir" || stop("no input data!");

[@config "/image/format"]
const format as string   = ?"--format"   || "png";
[@config "/image/ppi"]
const ppi as integer     = ?"--ppi"      || 300;
[@config "/image/filename"]
const filename as string = ?"--filename" || "test.png";

# test of missing in config.json
[@config "/missing/in/config.json"]
const missing1 a string = ?"--missing-in-config.json" || 12345;

# test of no config mapping
const noConfigMapping as string = ?"--no-config-mapping" || 33333;

print(`data input dir: ${dir}`);
print(`image output: '${dir}/${filename}' in format of ${format} and image resolution ppi value is '${ppi}'`);

print("test situations:");
print("missing in config.json:");
print(missing1);

print("no config.json mapping:");
print(noConfigMapping);

pause();