li = {
    a1: list(x=1,y="A"),
    a2: list(x=30,y="B"),
    a3: list(x=5,y="B"),
    a4: list(x=7,y="A"),
    a5: list(x=88,y="A"),
    a6: list(x=10,y="A"),
    a7: list(x=-1,y="C"),
    a8: list(x=-101,y="C")
};

print("raw list");
str(li);

# order by x
print("list order by x:");
str(li |> orderBy(i -> i$x));

# filter by x
print("list filter by x greater than zero!");
print("and then order by x desc:");
print("and then take top 3:");

str(

li 
|> which(i -> i$x > 0) 
|> orderBy(i -> i$x, desc = TRUE) 
|> head(n = 3)

);

# group by y
print("list group by y:");
str(li |> groupBy(i -> i$y));