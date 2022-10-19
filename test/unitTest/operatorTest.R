a = 1.0:100000.0;
b = 7.0:100007.0;
z = 9.0;

const add = function(x, y)print(x + y); 
const divide = function(x, y)  print(x / y);
const exponent = function(x, y)  print(x ^ y);
const modulo = function(x, y)  print(x % y);
const multiply = function(x, y)  print( x * y);
const subtract = function(x, y)print( x - y);

test = function(op) {
print(op);

op(a,b);
op(a,z);
op(z,a);
op(z,z);
op(b,a);
op(b,z);
op(z,b);
op(a,a);
op(b,b);

print("--------------------------------------");
cat("\n\n");
}

test(op = add);
test(op = divide);
test(op = exponent);
test(op = modulo);
test(op = multiply);
test(op = subtract);