require(geometry2D);
require(graphics2D);

setwd(@dir);

let outline = function(data) {
	data.frame(x = data$X, y= data$Y) |> concaveHull(as.polygon = TRUE);
}
let a = read.csv("./region_11.csv", row.names = NULL) |> outline;

bitmap(file = "./region_alpha_shapes.png") {
	charts::fillPolygon(a, reverse = TRUE);
}
