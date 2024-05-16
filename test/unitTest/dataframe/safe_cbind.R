let a = data.frame(a = 1:5);
let b = data.frame(b = 1:3);

print(a);
print(b);

print(cbind(a,b, strict = FALSE, default = -1));