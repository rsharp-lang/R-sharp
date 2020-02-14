require(base.math);
require(plot.charts);

solve.RK4(x -> x+1) :> plot :> save.graphics(file = "./demo.png");