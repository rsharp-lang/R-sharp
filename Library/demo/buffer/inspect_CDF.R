require(netCDF.utils);

options(strict = FALSE);

using cdf as open.netCDF("F:\20211123_CDF\P210702366.cdf") {
	print(cdf |> dimensions);
	print(cdf |> globalAttributes);
	print(cdf |> variables);
	
	print(cdf |> var("total_intensity"));
}
