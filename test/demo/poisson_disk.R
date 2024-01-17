let xy = poisson_disk();

xy[, "scale"] = 1;

print(xy);

bitmap(file = `${@dir}/poisson_disk_native.png`, size = [256,256]) {
    image(xy);
}