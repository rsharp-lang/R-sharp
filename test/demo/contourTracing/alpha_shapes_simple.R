let x = runif(500,20 , 900);
let y = runif(500,20, 900);
let outline = data.frame(x, y) |> concaveHull(as.polygon = TRUE);

print(outline);

bitmap(file = "./simple_alpha_shapes.png") {
	charts::fillPolygon(outline);
}
