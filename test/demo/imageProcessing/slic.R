imports "bitmap" from "MLkit";

let data = readImage(file.path(@dir, "lena.jpg" ));
let regions = bitmap::slic(data,region_size = 0.05,
                         iterations = 2);

regions = as.data.frame(regions);

setwd(@dir);

bitmap(file = "./lena.png") {
    plot(regions$x, regions$y, class = regions$cluster  );
};

