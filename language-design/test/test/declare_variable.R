let x as integer = 999;  # integer vector only have one element
let y = [1, 2, 3];       # integer vector have 3 elements

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

let t as double  = [100:1, -0.5];

# type constraint error: object type can not be convert to an integer vector
let i as integer = [t="g2", n=666, s=FALSE];

declare function test.global(x) {
    return x + [x];  
}

test.variable_not_found <- 123;
# object not found error: you must declare a variable using var statement before you use it!
