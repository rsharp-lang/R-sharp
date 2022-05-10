# warming up cpu clock
lapply(1:10000, x -> x);

@profile {
    x = [1.1,2.2,3.3,4.4,5.5,6.6,7.7,8.8,9.9,10.01];
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