setwd(!script$dir);

# view files
list.files("demo.zip");

# get file content
using zip as open.zip("demo.zip") {
	let raw = zip[["demo.csv"]] :> read.csv;
	
	print(raw);
}
