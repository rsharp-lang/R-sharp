let a = data.frame("" = 1:5, x = TRUE, c = ["adas","wqeqwe","adas","fafa","adasdas"]);

print(a);

print("delete a column:");
a[, ""] <- NULL;
print("delete a column should not trigger the error:");
a[, ""] <- NULL;

print(a);