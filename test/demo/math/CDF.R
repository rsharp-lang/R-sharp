options(strict = FALSE);

# Create a sequence of numbers between -10 and 10 incrementing by 0.1.
x <- seq(-10, 10, by = .1);
# Choose the mean as 2.5 and standard deviation as 0.5.
y <- dnorm(x, mean = 2.5, sd = 0.5);

pdf(file = `${@dir}/dnorm.pdf`) {
	plot(x, y, background = "white", grid.fill = "white", color = "steelblue", point_size = 16);
}

svg(file = `${@dir}/dnorm.svg`) {
	plot(x, y, background = "white", grid.fill = "white", color = "steelblue", point_size = 16);
}

let ecdf = CDF(x -> dnorm(x, mean = 2.5, sd = 2), [min(x), max(x)], resolution = 120);
	
	str(ecdf);

pdf(file = `${@dir}/CDF.pdf`) {
	plot(ecdf$x, ecdf$y, grid.fill = "white", color = "steelblue", point_size = 16);
}

svg(file = `${@dir}/CDF.svg`) {
	plot(ecdf$x, ecdf$y, grid.fill = "white", color = "steelblue", point_size = 16);
}