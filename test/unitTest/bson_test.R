require(JSON);

datafile = `${@dir}/test.dat`;
data = {

	path: "\\unc\path\to\file\aaa.txt"

};

write.bson(data, file = datafile);

str(parseBSON(readBin(datafile)));