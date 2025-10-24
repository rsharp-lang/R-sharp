require(geometry2D);
require(graphics2D);

setwd(@dir);

let outline = function(data) {
	data.frame(x = data$X, y= data$Y) |> concaveHull(r = 2, as.polygon = TRUE);
}

const a = read.csv("./region_11.csv", row.names = NULL) |> outline;
const b = read.csv("./region_2.csv", row.names = NULL) |> outline;
const c = read.csv("./region_9.csv", row.names = NULL) |> outline;

bitmap(file = "./region_alpha_shapes.png") {
	charts::fillPolygon(list(A = a, B = b, C = c), reverse = TRUE);
}
