var x <- 123;         # integer vector only have one element
var y <- {1, 2, 3};   # integer vector have 3 elements

print(x);
# [1] 123

print(y);
# [1] 1 2 3

typeof(x);
# integer

typeof(y);
# integer

typeof(x) is typeof(y);
# [1] TRUE

var t as double <- [100:1,-0.5];
var i as integer <- [t="g2", n=666, s=FALSE];
# type constraint error: object type can not be convert to an integer vector

test.global <- function(x) {
    return x + [x];  
}

test.variable_not_found <- 123;
# object not found error: you must declare a variable using var statement before you use it!
