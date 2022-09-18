obj = lapply(1:3000, x -> list(aaa = x));

@profile {
	# test lambda
	vec1 = sapply(obj, i -> i$aaa);
}

profile = profiler.fetch() |> as.data.frame();

print(profile);
print(sum(profile[, "ticks"]));

@profile {

	# test function
	vec2 = sapply(obj, function(i) {
		i$aaa;
	});
}

profile = profiler.fetch() |> as.data.frame();

print(profile, max.print = 6);
print(sum(profile[, "ticks"]));

@profile {

	# test vector loop
	vec3 = obj@aaa;
}

profile = profiler.fetch() |> as.data.frame();

print(profile, max.print = 6);
print(sum(profile[, "ticks"]));