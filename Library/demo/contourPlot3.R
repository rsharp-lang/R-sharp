imports "charts" from "graphics";

require(JSON);

bitmap(file = `${dirname(@script)}/contour3.png`) {
	`${dirname(@script)}/contour_layers_0146a__save.json`
	|> readText
	|> JSON::json_decode(schema = type("contours"))
	|> plot(colorSet = "Jet")
	;
}