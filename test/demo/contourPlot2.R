imports "charts" from "graphics";

bitmap(file = `${dirname(@script)}/1_Contour.png`) {
	`${dirname(@script)}/1_Contour.csv`
	|> read.csv(row.names = NULL)
	|> contourPlot(colorSet = "Jet")
	;
}