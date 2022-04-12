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
const missing1 as string = ?"--missing-in-config.json" || 12345;
# test of no config mapping
const noConfigMapping as string = ?"--no-config-mapping" || 33333;
# test of logical
const logical1 as boolean = ?"--flag1";
[@config "/render/blur"]
const logical2 as boolean = ?"--flag2";
[@config "/render/grayscale"]
const logical3 as boolean = ?"--flag3";

print(`data input dir: ${dir}`);
print(`image output: '${dir}/${filename}' in format of ${format} and image resolution ppi value is '${ppi}'`);

print("test situations:");
print("missing in config.json:");
print(missing1);

print("no config.json mapping:");
print(noConfigMapping);

print("test of the logcial flags in the config mapping");
print("flag1:");
print(logical1);

print("flag2:");
print(logical2);

print("flag3:");
print(logical3);

pause();