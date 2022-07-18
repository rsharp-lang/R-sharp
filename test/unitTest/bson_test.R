require(JSON);

datafile = `${@dir}/test.dat`;
data = {

	path: "\\unc\path\to\file\aaa.txt"

};

writeBSON(data, file = datafile);

str(parseBSON(readBin(datafile)));