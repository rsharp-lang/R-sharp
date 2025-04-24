let l = list("4333333" = FALSE,"yes", c(1,2,3,4,5));



# str(l);

# print(l[["4333333"]]);
# print(l[[3]]);

# str(l[[2]]);
# str(l[[1]]);

l[[5]] = c("aa", "bb");

str(l);

str(l[[5]]);

print(names(l));