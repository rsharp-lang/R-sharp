let x0 = seq(-2,0.47,length_out = 1000);
let y0 = seq(-1.12,1.12, length_out = 1000);
let x = 0;
let y = 0;
let x1 = 0;

for(let i in 1:20) {
    x1 =  x^2 - y^2 + x0;
    y = 2*x*y + y0;
    x = x1;

    bitmap(file = file.path(@dir, "iterations", `${i}.png`)) {
        plot(x,y, fill = "white", point.size = 3);
    }
}