imports "charts" from "R.plot";

bitmap(file = `${dirname(@script)}/demo-math-plot.png`) {
	plot(x -> x ^ 2, x = 1:16 step 0.1, size = [3600,2400], padding = [150, 150, 200, 300], grid.fill = "white");
}

print("run plot test success!");