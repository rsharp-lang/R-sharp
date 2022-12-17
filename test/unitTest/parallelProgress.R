# list = as.list( [4,5,6,7,8]);
list = as.list(1);

str(list);

parLapply(list, function(x) {
    sleep(x);
    progress(0);
    sleep(x);
    progress(25);
    sleep(x);
    progress(50);
    sleep(x);
    progress(75);
    sleep(x);
    progress(100);

    x ^ 2;
}) 
|> str();

