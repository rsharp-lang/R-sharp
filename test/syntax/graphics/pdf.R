require(graphics2D);

# demo script for create pdf image

pdf(file = `${@dir}/colors.pdf`) {
	maps = colorMap.legend(
		colors = "viridis:turbo", ticks = 0:100 step 25, title = "Intensity Scale"
	);
	
	plot(maps, size = [200,400], padding = [10, 10, 20, 30]);
}