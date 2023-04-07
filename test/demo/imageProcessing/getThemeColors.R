setwd(@dir);

img = readImage("./906338a89185d86bf93650eab1df27bffb34ee06.jpg");

theme_colors = colors(img, n = 8);

print(theme_colors);

