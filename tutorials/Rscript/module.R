# module in R# is a kind of code collection for group 
# your function into a collection for implements a specific code function
# module is similar to package namespace

module test

	# module can be simulated by a closure
	# it have its own environment
	let x as integer = [9,99,999];
	
	let setX as function(x) {
		test::x <- x;
	}

	let getX = [] -> x;

end module

# using module test

print(test::getX());
print(test::x);

[1,3,5,7,9] :> test::setX;

print(test::getX());
print(test::x);