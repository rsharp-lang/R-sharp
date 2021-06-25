const a <- [1,2,3,4,5];
const b <- [5,7,9,11,13];
const c <- [15,31.5,1.8,1.5,71.5];
const d <- [3,3,3,3,3.3];
const e <- [3,3,3,3,3.3];
const f <- [3,3,3,3,3.3];
const g <- [3,3,3,3,3.3];
const h <- [3,3,3,3,3.3];
const i <- [3,3,3,3,3.3];
const j <- [3,3,3,3,3.3];
const k <- [3,3,3,3,3.3];
const mydata <- data.frame(a, b,c, d, e,f,g,h,i,j,k);

const mlr = lm(b ~ a + c + d, mydata);

print(mlr);

print(mlr :> predict(mydata));


print(lm(b ~ a + c + d +e +f +g +h +i+ j+k, mydata));