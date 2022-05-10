# warming up cpu clock
lapply(1:10000, x -> x);

d = 3;
i = 200;

# @profile {
#     x = 1:5000 step 1.1;
#     a = x[i];

#     str(x);
    
#     r = lapply(1:100, any -> which(abs(a - x) < d));

#     print("assert result:");
#     print(a);
#     print(x[r[[1]]]);
#     print(abs(x[r[[1]]] - a));
# }

# cat("\n\n");

# profiler.fetch() 
# |> as.data.frame() 
# |> print(select = ["ticks","elapse_time","expression"])
# ;

@profile {
    x = 1:5000 step 1.1;
    a = x[i];

    str(x);
    
    x = blockIndex(x, d, any -> any);
    r = lapply(1:100, any -> x |> blockQuery(a));

    print("assert result:");
    print(a);
    print(r[[1]]);
    print(abs(r[[1]] - a));
}

cat("\n\n");

profiler.fetch() 
|> as.data.frame() 
|> print(select = ["ticks","elapse_time","expression"])
;