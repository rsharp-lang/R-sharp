li = list(
    a1 = list(x=1,y="A"),
    a2 = list(x=3,y="B"),
    a3 = list(x=5,y="B"),
    a4 = list(x=7,y="A"),
    a5 = list(x=88,y="A"),
    a6 = list(x=10,y="A"),
    a7 = list(x=-1,y="C"),
    a8 = list(x=-101,y="C")
);

str(li);

# order by x
str(li |> orderBy(i -> i$x));

# group by y
str(li |> groupBy(i -> i$y));