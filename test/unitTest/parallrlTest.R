a = 1:100;
a = as.list(a, names = `X_${a}`);

b = parLapply(a, x -> x ^ 2, n_threads = 4, names = `new_name_${names(a)}`);

str(b);

