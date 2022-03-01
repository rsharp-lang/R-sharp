setwd(@dir);

bitmap(file = "./duplicated.png", size = [3200,2400]);

rasterImage(readImage("./demo-math-plot.png"), 5,5, [160,300]);

dev.off();