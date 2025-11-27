let x = tryCatch(system2("R#.exe", "--help"), finally = print("Hello"));

print(x);