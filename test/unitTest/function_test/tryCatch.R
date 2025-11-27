let x = tryCatch(system2("R#.exe", "--help"), finally = print("Hello"));

print(x);

x= tryCatch(stop(1), error = function(x) 666);