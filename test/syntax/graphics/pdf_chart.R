pdf(file = `${@dir}/plotpdf.pdf`) {
	plot(x -> x ^ 3, x = (-10):16 step 0.5, size = [3600,2400], padding = [150, 150, 200, 300], grid.fill = "white");
}