let a <- [1,2,3,4,5];
let b <- [5,7,9,11,13];
let c <- [15,31.5,1.8,1.5,71.5];
let d <- [3,3,3,3,3.3];
let mydata <- data.frame(a, b,c, d);

let mlr = lm(b ~ a + c + d, mydata);

print(mlr);

print(mlr :> predict(mydata));