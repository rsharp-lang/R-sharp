imports "buffer" from "base";

const buffer = readBin(`${dirname(@script)}/20210207.sld`);
const data = buffer[buffer != 0];
const chars = string(data[(data >= 32) && (data < 128)]);

print("decoded chars from the raw data buffer:");
str(chars);

const json_list = $"[{].+?[}]"(chars);

print(json_list);