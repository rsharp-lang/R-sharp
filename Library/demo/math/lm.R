let a <- [1,2,3,4,5];
let b <- [2,4,6,8,10];
let mydata <- data.frame(a, b);

print("weighted formula:");
print(lm(b ~ a, mydata, weights = 1 / a));

print("unweighted:");
print(lm(b ~ a, mydata));