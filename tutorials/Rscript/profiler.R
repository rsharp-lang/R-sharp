hypot = function(x, y) {
[x, y] = abs([x, y]);

if (x > y) {
	r = y /x;
	return x * sqrt(1+r ^ 2);
}
if (y == 0) {
	return 0;
}

r = x/y;

return y*sqrt(1+r ^ 2);
}

@profile {
	for(i in 1: 5) {
		print(hypot(i, 3));
	}
}

print(" ~~done!");