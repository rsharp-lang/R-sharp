# A lambda function is a very simple function closure
# with function body just one or zero parameter
# and a simple function body with only one line of R# code 

# verbose
let add1 as function(x) {
	`x+1 = ${x+1}`;
}

print(add1(99));

let seed as integer = 0;

let populate as function() {
	seed;
}

let addTuple as function(x,y) {
	x+y;
}

# elegant
let add1.lambda = x => x+1;
let populate.lambda = [] => seed;
let addTuple.lambda = [x,y] -> x+y;