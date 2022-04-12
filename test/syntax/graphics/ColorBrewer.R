require(ColorBrewer);

for(name in ls("package:ColorBrewer")) {
	print("color set of schema:");
	print(name);
	print(do.call(name));
}