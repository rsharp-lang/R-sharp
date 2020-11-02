let a <- [1,2,3,4,5];
let b <- [2,4,6,8,10];
let mydata <- data.frame(a, b);

let w = lm(b ~ a, mydata, weights = 1 / a);
let uw = lm(b ~ a, mydata);

print("weighted formula:");
print(w);

print("unweighted:");
print(uw);

print(as.formula(uw));