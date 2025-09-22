let x = file(relative_work("aaa.txt"), open = "r");

print(readLines(x, n = 1));
print(readLines(x));