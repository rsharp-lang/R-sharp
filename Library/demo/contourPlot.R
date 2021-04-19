imports "charts" from "graphics";

bitmap(file = `${dirname(@script)}/contour.png`) {
	contourPlot([x,y] => x ^ 2 + y ^ 5, x = [-1, 1], y = [-1,1]);
}