require(netCDF.utils);

options(strict = FALSE);
options(max.print = 100);

using cdf as open.netCDF("F:\20211123_CDF\P210702366.cdf") {
	print(cdf |> dimensions);
	print(cdf |> globalAttributes);
	print(cdf |> variables);
	
	print(cdf |> var("total_intensity"));
	print(cdf |> attr("total_intensity"));
	print(cdf |> attr("mass_values"));
	print(cdf |> attr("intensity_values"));
	
	print(cdf |> var("scan_index") |> getValue());
}
