data = data.frame(
	x = sin(1:5),
	y = cos(5:9)
);

print(data);
print(typeof(data));

if (typeof(data) is "string") {
	stop("wrong!");
} else {
	print("yes!");
}

data = ["a","bb","ccc","dddd"];

print(data);
print(typeof(data));

if (typeof(data) is "string") {
	print("yes!");
} else {
	stop("wrong!");
}