let f = function(x, A,B, C) {
    A / (1 + exp(-B * (x - C)));
}

let x = (-3):20 step ((20+3)/100);
let y = f(x, 20, 1 , 5) + runif(length(x), -1.5, 1.5);

x = x + runif(length(x), -1.5, 1.5);

let pars = curve_fit(f, x, y, A = 1, B = 1, C = 1);

str(pars);
setwd(@dir);

# stop();

bitmap(file = "./sigmoid_fit.png", fill = "white", padding = [100, 200, 100, 200]) {
    plot(x, y, line = FALSE, color = "blue",
         fit1 = list(x = x, y = f(x, pars$A, pars$B, pars$C), color = "red"));
}