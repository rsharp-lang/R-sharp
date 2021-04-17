imports "charts" from "graphics";

bitmap(file = `${dirname(@script)}/formula.png`) {
	contourPlot(~x ^ 2 + y ^ 3, x = [-1, 1], y = [-1,1]);
}