setwd(@dir);

img = readImage("./906338a89185d86bf93650eab1df27bffb34ee06.jpg");

theme_colors = colors(img, n = 8,character  = TRUE);

print(theme_colors);

rect = `<div style="width: 50px; height: 50px; background-color: ${theme_colors}"></div>`;

writeLines(rect, con = "./theme_colors.html");

# pause();