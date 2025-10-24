let x = runif(100,20 , 900);
let y = runif(100,20, 900);
let outline = data.frame(x, y) |> concaveHull(r = 5, as.polygon = TRUE);

print(outline);

bitmap(file = "./simple_alpha_shapes.png") {
	charts::fillPolygon(outline);
}
