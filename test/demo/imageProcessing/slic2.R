imports "bitmap" from "MLkit";

let data = readImage(file.path(@dir, "/../1537192287563.jpg" ));
let regions = bitmap::slic(data,region_size = 0.3,
                         iterations = 10);

regions = as.data.frame(regions);

setwd(@dir);

bitmap(file = "./demo2.png") {
    plot(regions$x, regions$y, 
        class = regions$cluster, 
        colors = "paper", 
        point.size = 5, 
        reverse=TRUE  );
};

