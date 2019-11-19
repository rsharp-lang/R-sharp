# Demo about how to declare a variable in R# 

# declare a variable with NULL;
# variable type is generic
let x;

# declare a variable with type;
let str as string;

# declare a variable with initialize value
let y as integer = 1:13;

# value assign
x <- y;
str <- `integer value of y is ${y}`;

print(x);
print(y);
print(str);

# get variable by name
print(get("x"));