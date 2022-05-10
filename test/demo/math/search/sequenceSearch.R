# warming up cpu clock
lapply(1:10000, x -> x);

@profile {
    x = [1,2,3,4,5,6,7,8,9,0];
    i = 5;
    a = x[i];

    str(x);

    x = sapply(1:20000, any -> which(a == x));

    print("assert result:");
    print(all(x == i));
}

profiler.fetch() 
|> as.data.frame() 
|> print(select = ["ticks","elapse_time","expression"])
;

@profile {
    x = [1,2,3,4,5,6,7,8,9,0];
    i = 5;
    a = x[i];

    str(x);

    x = binaryIndex(x);
    x = sapply(1:20000, any -> binarySearch(x, a));
print(x);
    print("assert result:");
    print(all(x == i));
}

profiler.fetch() 
|> as.data.frame() 
|> print(select = ["ticks","elapse_time","expression"])
;