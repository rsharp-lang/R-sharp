setwd(@dir);

extract_colors = function(file) {
    img = readImage(file);
    theme_colors = colors(img, n = 6,character  = TRUE);
    rect = `<div style="width: 50px; height: 50px; background-color: ${theme_colors}"></div>`;
    rect = `
        <div style="width: 5%; min-width: 50px; float: left;">
        ${paste(rect, " ")}
        </div>
        <div style="width: 70%;"><img style="height: 600px;" src="${file}"></div>
    `;
    print(theme_colors);
    writeLines(rect, con = `${basename(file)}.html`);
}

# pause();

extract_colors("./6fcc9fe5f3e1324ebad1dffff1d80cce3a0e2e07.jpg");
extract_colors("./906338a89185d86bf93650eab1df27bffb34ee06.jpg");