l = list(a = 123,zzz = 666, b = FALSE, cdf = c(3, 45), ggt = "sfsdfd");
s = l[order(names(l))];

print(names(l));
print(order(names(l)));

str(l);
str(s);

a = rev(1:6);

print(a[c(3,5,6,1,1,1,2,4)]);
