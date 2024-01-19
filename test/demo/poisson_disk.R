require(graphics);

let xy = poisson_disk();

xy[, "scale"] = 1;

setwd(@dir);
print(xy, max.print = 6);

bitmap(file = "./poisson_disk_native.png", size = [256,256]) {
    image(xy);
}

xy = poisson_disk(dart = readImage("./1537192287563.jpg"));
xy[, "scale"] = 1;

print(xy, max.print = 6);

bitmap(file = "./poisson_disk_scaled.png", size = [256,256]) {
    image(xy);
}