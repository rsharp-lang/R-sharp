let add.abs as function(x, y) {
	if(x < 0) {
		x <- x * (-1);
	}
	if (y < 0) {
		y <- y * (-1);
	}
	
	return x + y;
}

print(add.abs);