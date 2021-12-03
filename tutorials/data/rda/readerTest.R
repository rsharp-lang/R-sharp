options(strict = FALSE);

testdir = "E:\GCModeller\src\R-sharp\studio\test\data";
file = [`${testdir}\test_dataframe2.rda`,
`${testdir}\multiple_object.rda`,
`${testdir}\test_dataframe.rda`,
`${testdir}\test_dataframe_v3.rda`];

for(path in file) {
	list = readRData(path);
	
	for(name in names(list)) {
		print("symbol name:");
		print(name);
		
		cat("\n");
		
		if (typeof list[[name]] is "data.frame") {
			print(list[[name]]);
		} else {
			str(list[[name]]);
		}
		
		cat("\n\n");
	}
	
	print(path);
	cat("\n\n\n");
}