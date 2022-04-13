options(strict = FALSE);

# Create a sequence of numbers between -10 and 10 incrementing by 0.1.
x <- seq(-10, 10, by = .1);
# Choose the mean as 2.5 and standard deviation as 0.5.
y <- dnorm(x, mean = 2.5, sd = 0.5);

bitmap(file = `${@dir}/dnorm.png`) {
	plot(x, y, background = "white", grid.fill = "white", color = "steelblue", point_size = 16);
}

bitmap(file = `${@dir}/CDF.png`) {
	ecdf = CDF(x -> dnorm(x, mean = 2.5, sd = 2), [min(x), max(x)], resolution = 120);
	
	str(ecdf);
	plot(ecdf$x, ecdf$y, grid.fill = "white", color = "steelblue", point_size = 16);
}