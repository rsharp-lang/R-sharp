alist = list(

    a = list(x = 1111),
    b = list(x = 9999),
    c = list(x = 5555)
);

symbol = "x";

print(alist@{symbol});

print(alist@x);
