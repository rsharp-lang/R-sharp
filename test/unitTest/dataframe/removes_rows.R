let f(x) = x^3;
let y = f(1:5);
a= data.frame(x = 1:5, y = y, z = FALSE, m = y % 3 == 0);

print(a);

print("dataframe removes row 3:");
print(a[-3,]);