a = data.frame(i = `#${1:5}`, A = ['a','c','z','gg','A']);

print(a);

index = order(a[, "i"], decreasing = TRUE);
index = index[1:64];

print(nrow(a));
print(index);

a = a[index, ];