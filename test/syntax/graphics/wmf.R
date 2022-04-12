# wmf(file = `${@dir}/plot.wmf`) {
	# plot(x -> x ^ 3, x = (-10):16 step 0.5, size = [3600,2400], padding = [150, 150, 200, 300], grid.fill = "white");
# }


# demo test 2
wmf(file = `${@dir}/plot2.wmf`);
plot(x -> x ^ 3, x = (-10):16 step 0.5, size = [3600,2400], padding = [150, 150, 200, 300], grid.fill = "white");
dev.off();
