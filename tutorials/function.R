let add.simple = [x,y] -> x+y;

let add.abs as function(x, y) {
	if(x < 0) {
		x <- x * (-1);
	}
	if (y < 0) {
		y <- y * (-1);
	}
	
	add.simple(x , y);
}

# print user function which is declared in 
# current R# script
print(add.abs);

# print a user lambda function 
print(add.simple);

# print api function which is declared in a 
# package library module file
print(list);