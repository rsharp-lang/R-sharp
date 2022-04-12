options(strict = FALSE);

setwd("E:\GCModeller\src\R-sharp\studio\test\data");

list = base::readRData("multiple_object.rda");
	
print("get all symbols in target RData dataset:");
print(names(list));
	
print("===============================================");
	
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