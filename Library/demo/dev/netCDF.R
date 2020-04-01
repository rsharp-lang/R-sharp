imports "netCDF.utils" from "R.base";

using file as open.netCDF("D:\GCModeller\src\R-sharp\tutorials\io\R#save.rda") {
	print(file :> globalAttributes);
}

