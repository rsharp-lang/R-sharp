imports "http" from "webKit";

require(JSON);

const url  = ?"--url" || stop("no url data was provided!");
const type = ?"--content-type" || "json";
const page = url |> requests.get() |> http::content();

if (is.character(page)) {
	if (type == "json") {
		str(JSON::json_decode(page));
	} else {
		str(page);
	}
} else {
	str(page);
}

