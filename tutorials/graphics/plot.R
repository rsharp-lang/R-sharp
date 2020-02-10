imports "plot.charts" from "R.plot.dll";

plot(x -> x ^2 , x= 1: 16 step 0.5)
:> save.graphics(file = "demo-math-plot.png")
;