a = data.frame(i = `#${1:3}`, A = ['a','c','z']);

print(a);

print(a[c(1,2,3, 3,1, 5), ]);
print(a[c(TRUE,TRUE,FALSE), ]);

print(a[4, ]);