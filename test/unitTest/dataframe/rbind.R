let a = data.frame(a = "a":"c", b = TRUE, c = [888, 999, -1]);

print(a);
print(rbind(a, data.frame(a = ["xx" "YY"], b = FALSE, c = -99999)));


print(rbind(a, ["XXXX",  FALSE,   -10]));


print(rbind(NULL, 1:5));
print(rbind(NULL, ["8888", TRUE, -9]));
print(rbind(NULL, list(a = 1, b = FALSE, c = "ooooo")));

