require(base.math);
require(plot.charts);

setwd(!script$dir);

let a = neg(8/3);
let b = neg(100);
let c = 28;

let lorenz = [
	x -> a*x + y*z,
	y -> b*(y-z),
	z -> c*y -x*y -z
];

lorenz 
:> deSolve( list(x = 1,y = 1, z = 1), a = 0, b= 10 ) 
:> as.data.frame 
:> write.csv(file = "./lorenz.csv");