imports "netCDF.utils" from "R.base";

using zip as open.zip("D:\GCModeller\src\R-sharp\tutorials\io\R#save.rda") {
	using file as open.netCDF(zip[["R#.Data"]]) {
		print(file :> globalAttributes);
		
		print("this r dataset file contains symbols:");		
		print(file :> variables);
	}
}


