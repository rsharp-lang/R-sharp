let vl = list(

    a = 1:100,
    b = runif(100,50, 60),
    c = runif(100, 1, 2)

);

str(vl);

print(vl@{1});
print(vl@{10});
print(vl@{100});