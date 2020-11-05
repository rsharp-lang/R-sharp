let a <- [1,2,3,4,5];
let b <- [5,7,9,11,13];
let c <- [15,31.5,1.8,1.5,71.5];
let d <- [3,3,3,3,3.3];
let e <- [3,3,3,3,3.3];
let f <- [3,3,3,3,3.3];
let g <- [3,3,3,3,3.3];
let h <- [3,3,3,3,3.3];
let i <- [3,3,3,3,3.3];
let j <- [3,3,3,3,3.3];
let k <- [3,3,3,3,3.3];
let mydata <- data.frame(a, b,c, d, e,f,g,h,i,j,k);

let mlr = lm(b ~ a + c + d, mydata);

print(mlr);

print(mlr :> predict(mydata));


print(lm(b ~ a + c + d +e +f +g +h +i+ j+k, mydata));