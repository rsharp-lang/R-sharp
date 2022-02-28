require(graphics2D);

wmf(file = `${@dir}/colors.wmf`) {
	maps = colorMap.legend(
		colors = "viridis:turbo", ticks = 0:100 step 10, title = "Intensity Scale"
	);
	
	plot(maps, size = [200,400], padding = [10, 10, 20, 30]);
}