
const export = function(r, a = FALSE, b = 333, d = "hello") {
	print(getOption("lalala"));
	print(as.logical(getOption("cache.enable")));

	str(list(r, a, b, d));
}