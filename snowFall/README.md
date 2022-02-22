# Difference between ``R#`` parallel and parallel foreach

## foreach parallel

foreach parallel is a **build-in** parallel function in ``R#`` environment, it run depends on the thread parallel in the same process. which it means all of the task worker code run in parallel is in the same ``R#`` process but seperated by different task threads.

```r
# run loop in parallel
x = for(x in seq) %dopar% {
    # ...
}

# run loop in sequence
x = for(x in seq) {
    # ...
}
```

## snowfall parallel

the snowfall parallel in ``R#`` environment is a kind of thrid-part parallel api, it seperates the parallel task worker into different Rscript process.

```r
x = parallel(x = seq) {
    # task worker code run in different Rscript process
    # ...
}
```

