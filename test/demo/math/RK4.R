require(base.math);
require(plot.charts);

setwd(!script$dir);

solve.RK4(x -> x^2 + 1) 
:> plot 
:> save.graphics(file = "./demo.png");