# demo script for create a list in R#

let x as integer = 99;
let b as boolean = [TRUE, TRUE, TRUE, TRUE, TRUE, FALSE];
let list = list(x = x, flags = b, inner = list(a = x, b =x *100));

print(x);
print(b);
print(list);