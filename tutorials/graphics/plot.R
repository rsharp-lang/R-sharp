imports "charts" from "R.plot";

bitmap(file = `${dirname(@script)}/demo-math-plot.png`) {
	plot(x -> x ^2 , x= 1: 16 step 0.5);
}