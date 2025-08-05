let data = data.frame(A = [1 2 3 4 5], B = TRUE,X=["A" "C" "CH" "DDD" "ERE"]);

print(data);
print(data[, c("A","B","C"), strict = FALSE]);
print(data[, c("A","B","C")]);