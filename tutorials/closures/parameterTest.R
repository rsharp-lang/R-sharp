bitmap(file = `${dirname(@script)}/plot.png`, width = 300, height = 300, dpi = 300) {
	print("run test!");
    plot(1:5 step 0.1);
}