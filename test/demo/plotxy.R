let x = (-10):10 step 0.1;
let f(x) = x^3;

svg(file = file.path(@dir,"plotxy.svg")) {
    plot(x,f(x));
}