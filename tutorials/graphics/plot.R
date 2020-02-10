require(plot.charts);

plot(x -> x ^2 , 1: 16 step 0.5);
:> save.graphics(file = "demo-math-plot.png")
;