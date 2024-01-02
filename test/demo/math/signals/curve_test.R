let f = function(x, a, b, c) {
    a * sin((2 * PI * x) / 12 + b) + c;
}

let x = 1:12;
let x2 = 1:13 step 0.1;
let y = [17 19 21 28 33 38 37 37 31 23 19 18];

let pars1 = curve_fit(f, x, y, a = 1, b = 1, c = 1);
let pars2 = curve_fit(f, x, y, args = [1 1 1]);

str(pars1);
print(pars2);

setwd(@dir);

# stop();

bitmap(file = "./curve_fit.png", fill = "white") {
    plot(x, y, line = TRUE, fit1 = list(x = x2, y = f(x2, pars1$a, pars1$b, pars1$c)));
}