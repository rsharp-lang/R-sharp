a = 1.0:1000000.0;
ca =a;
b = 7.0:1000006.0;
cb = b;
z1 = 9.0;
z2 = 9.0;

const add = function(x, y)str(x + y); 
const divide = function(x, y)  str(x / y);
const exponent = function(x, y)  str(x ^ y);
const modulo = function(x, y)  str(x % y);
const multiply = function(x, y)  str( x * y);
const subtract = function(x, y)str( x - y);

# print(z + z);
str(a);
str(b);
str(z1);

test = function(op) {
print(op);

op(a,b);
op(a,z1);
op(z1,a);
op(z1,z2);
op(b,a);
op(b,z1);
op(z1,b);
op(a,ca);
op(b,cb);

print("--------------------------------------");
cat("\n\n");
}

t1 = now();

@profile {
test(op = add);
test(op = divide);
test(op = exponent);
test(op = modulo);
test(op = multiply);
test(op = subtract);

}

t2 = now();

print(as.data.frame(profiler.fetch()));
print(t2 - t1);


