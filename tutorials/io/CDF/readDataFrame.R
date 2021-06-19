require(netCDF.utils);

using dataset as open.netCDF(`${dirname(@script)}/dataframe.netcdf`) {
	print("view of the meta data of this netcdf data file:");
	print(dataset |> globalAttributes);
	print(dataset |> dimensions);
	
	cat("\n\n");
	
	const varList = dataset |> variables;
	const table   = data.frame();

	print("view of the variable list that store in this cdf file:");
	print(varList);
	
	for(name in varList[, "name"]) {
		table[, name] = dataset
			|> var(name)
			|> getValue
			;
	}
	
	print("view of the dataframe result:");
	print(table);
}