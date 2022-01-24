require(ggplot);

imports "./hypot.jl";

# hypot = function(x, y) {
# [x, y] = abs([x, y]);

# if (x > y) {
	# r = y /x;
	# return x * sqrt(1+r ^ 2);
# }
# if (y == 0) {
	# return 0;
# }

# r = x/y;

# return y*sqrt(1+r ^ 2);
# }

@profile {
	run = function() {
		for(i in 1: 50) {
			print(hypot(i, 3));
			rep(1, times = 1000000);
			gc();
		}			

		NULL;
	}

	gc();
	
	print(run());
}

print(" ~~done!");
profile = profiler.fetch() |> as.data.frame();
print(profile, max.print = 13);

write.csv(profile, file = `${@dir}/profile.csv`, row.name = TRUE);

profile[, "index"] = 1:nrow(profile);

# data visualize
bitmap(file = `${@dir}/memory_delta.png`) {
    ggplot(profile, padding = "padding: 200px 800px 250px 250px;", width = 4000, height = 2400) 
    + geom_line( aes(x = "index", y = "memory_delta"), width = 8)
	+ xlab("time(ticks)")
	+ ylab("memory_delta(MB)")
	+ ggtitle("Memory Delta Size")
    ;
}