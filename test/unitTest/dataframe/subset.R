a = data.frame(i = `#${1:5}`, A = ['a','c','z','b',"a"], row.names = "a":"e");

print(a);

print(a[a$A == 'a', ]);
print(a["b":"d", ]);


print(a[c(1,2,3, 3,1, 5), ]);
print(a[c(TRUE,TRUE,FALSE), ]);

print(a[4, ]);