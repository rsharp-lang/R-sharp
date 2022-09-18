obj = lapply(1:3000, x -> list(aaa = x));
vec1 = NULL;
vec2 = NULL;
vec3 = NULL;

@profile {
	# test lambda
	vec1 = sapply(obj, i -> i$aaa);
}

print(vec1);
	
profile = profiler.fetch() |> as.data.frame();
t1=sum(profile[, "ticks"]);

print(profile);
print(t1);

@profile {

	# test function
	vec2 = sapply(obj, function(i) {
		i$aaa;
	});

}

print(vec2);
	
profile = profiler.fetch() |> as.data.frame();
t2=sum(profile[, "ticks"]);

print(profile, max.print = 6);
print(t2);

@profile {

	# test vector loop
vec3 = 	obj@aaa;

}

print(vec3);
	
profile = profiler.fetch() |> as.data.frame();
t3 = sum(profile[, "ticks"]);

print(profile, max.print = 6);
print(t3);


print(t1/t3);
print(t2/t3);