imports "geometry2D" from "graphics";
imports "machineVision" from "signalKit";

setwd(@dir);

let raw = readImage("—Pngtree—five chickens in different colors_3632916.jpg");
let bin = machineVision::ostu(raw, factor = 0.8);
let shapes = machineVision::ccl(bin);

print(`find ${length(shapes)} shapes.`);

bitmap(bin, file = "ostu_bin.bmp");
bitmap(file = "shapes.png", size = [3600, 2700]) {
    plot(shapes, scatter = TRUE, padding = "padding: 5% 5% 10% 10%;");
}
