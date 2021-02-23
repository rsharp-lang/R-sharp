const buffer = readBin(`${dirname(@script)}/20210207.sld`);
const data = buffer[buffer != 0];
const chars = data[(data >= 32) && (data < 128)];

print(chars);