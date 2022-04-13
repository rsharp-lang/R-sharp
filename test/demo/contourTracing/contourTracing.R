require(graphics2D);

setwd(@dir);

const outline as function(data) {
	const x = data[, "X"];
	const y = data[, "Y"];
	
	contour_tracing(x, y, 5);
}

const a = read.csv("./region_11.csv", row.names = NULL) |> outline;
const b = read.csv("./region_2.csv", row.names = NULL) |> outline;
const c = read.csv("./region_9.csv", row.names = NULL) |> outline;

bitmap(file = "./region_unions.png") {
	charts::fillPolygon(list(A = a, B = b, C = c), reverse = TRUE);
}
