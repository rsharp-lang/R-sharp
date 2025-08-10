imports "charts" from "graphics";

let data = read.csv("Z:\\annotation.csv", row.names = 1, check.names = FALSE);

pdf(file = relative_work("pie.pdf")) {
    pie(data$adducts);
}