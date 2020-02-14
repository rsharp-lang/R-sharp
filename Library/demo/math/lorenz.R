require(base.math);
require(plot.charts);

setwd(!script$dir);

let sigma = 10;
let rho = 28;
let beta = 8 / 3;

let lorenz = [
	x -> sigma * (y - x),
    y -> x * (rho - z) - y,
    z -> x * y - beta * z
];

lorenz 
:> deSolve( list(x = 1,y = 1, z = 1), a = 0, b= 120 ) 
:> as.data.frame 
:> write.csv(file = "./lorenz.csv");