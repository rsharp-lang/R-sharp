require(netCDF.utils);

options(strict = FALSE);
options(max.print = 100);
options(memory.load = "max");

using cdf as open.netCDF("F:\20211123_CDF\P210702366.netcdf") {
	print(cdf |> dimensions);
	print(cdf |> globalAttributes);
	print(cdf |> variables);
	
	cat("\n\n\n");
	
	print("-------------------------------------------------------");
	
	print("get variable names:");
	print((cdf |> variables)[, "name"]);
	
	cat("\n\n\n");
	
	for(name in (cdf |> variables)[, "name"]) {
		print("load variable...");
		print(name);
	
		v = cdf |> var(name);
		
		print(v);
		print(cdf |> attr(name));
		print(v |> getValue());
		print("---------------------------------------------------");
		
		cat("\n\n\n");
	}
}
