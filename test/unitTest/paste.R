print(paste("a", "b", "c"));
# [1] "a b c"

print(paste(c("a1", "a2"), c("b1", "b2"), c("c1", "c2")));
# [1] "a1 b1 c1" "a2 b2 c2"

print(paste("a", "b", "c", sep = "-"));
# [1] "a-b-c"

print(paste(c("a1", "a2"), c("b1", "b2"), c("c1", "c2"), sep = "-"));
# [1] "a1-b1-c1" "a2-b2-c2"

print(paste(c("a", "b", "c")));
print(paste(c("a", "b", "c"), collapse = "//"));
# [1] "a" "b" "c"
# [1] "a//b//c"

print(paste("a", "b", "c", sep = "-", collapse = "//"));
# [1] "a-b-c"

print(paste(c("a1", "a2"), c("b1", "b2"), c("c1", "c2"), sep = "-"));
print(paste(c("a1", "a2"), c("b1", "b2"), c("c1", "c2"), sep = "-", collapse = "//"));
# [1] "a1-b1-c1" "a2-b2-c2"
# [1] "a1-b1-c1//a2-b2-c2"