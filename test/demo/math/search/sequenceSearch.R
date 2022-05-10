@profile {

    # warming up cpu clock
    lapply(1:10000, x -> x);

    x = [1,2,3,4,5,6,7,8,9,0];
    i = 5;
    a = x[i];

    str(x);

    x = sapply(1:10000, any -> i == which(a == x));

    print("assert result:");
    print(all(x));
}

profiler.fetch() 
|> as.data.frame() 
|> print()
;