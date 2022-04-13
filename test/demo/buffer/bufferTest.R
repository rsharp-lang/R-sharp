imports ["buffer", "JSON"] from "base";

const buffer = readBin(`${dirname(@script)}/20210207.sld`);
const data = buffer[buffer != 0];
const chars = string(data[(data >= 32) && (data < 128)]);

print("decoded chars from the raw data buffer:");
str(chars);

const json_list = $"[{].+?[}]"(chars);

print(json_list);

const filelist = lapply(json_list, json_decode);

# str(filelist);

data.frame(
	sample = sapply(filelist, function(file) file$originalRawFileNameWithoutExtension),
	injectionOrder = sapply(filelist, function(file) file$displayRowNumber)
)
:> write.csv(file = `${dirname(@script)}/seq.csv`, row_names = FALSE)
;
