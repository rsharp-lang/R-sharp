let a = data.frame(x = 1:10, y = runif(10), z = z(runif(10)));

print(a);

print(apply(a, margin = "row", sum));
print(apply(a, margin = "column", sum));