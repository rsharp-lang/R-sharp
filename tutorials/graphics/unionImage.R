setwd(@dir);

bitmap(file = "./duplicated.png", size = [3200,2400]);

rasterImage(readImage("./demo-math-plot.png"), 5,5, [600,400]);
text(1000, 500, "hello world!");

dev.off();


# pdf image file test

pdf(file = "./Rplot.pdf", size = [2100,1400], fill = "black");

rasterImage(readImage("./demo-math-plot.png"), 5,5, [600,400]);
text(1000, 500, "hello world!", col = "white");

dev.off();