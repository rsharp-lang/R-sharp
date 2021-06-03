const common_compute as function(x, type) {
	switch(type, default -> stop(`invalid method name '${type}'!`)) {
		mean   = mean(x),
		median = median(x),
		sum    = sum(x),
		max    = max(x),
		min    = min(x),
		sqrt   = sqrt(x),
		err    = {
			stop("this is an error test in switch expression!");
		}
	}
}

const x = [11, 23, 9, 49, 46, 92, 29, 3];

print(x |> common_compute("mean"));
print(x |> common_compute("median"));
print(x |> common_compute("sum"));
print(x |> common_compute("max"));
print(x |> common_compute("min"));
print(x |> common_compute("sqrt"));

# test other operation
# like throw error message
const catch = try(error -> x |> common_compute("err")) {
	# catch ex if this catch block is defined
	print(error);
}

common_compute(x, "blabla");