let a <- [1,2,3,4,5];
let b <- [2,4,6,8,10];
let mydata <- data.frame(a, b);
let myfit <- lm(a ~ b, mydata);

summary(myfit);