imports "geometry2D" from "graphics";
imports "machineVision" from "signalKit";

let multishapes = read.csv(system.file("data/multishapes.csv", package = "REnv"), row.names = NULL, check.names = FALSE);
let t0 = new geo_transform(
	theta = 1,
	tx = 3,
	ty = -5.5,
	scalex = 1.5,
	scaley = 0.9
);

print(multishapes, max.print = 16);
print("test geometry transform parameters:");
print(t0);

multishapes = polygon2D(multishapes$x,multishapes$y);

let test = geo_transform(multishapes, transform = t0);

print(multishapes);
print(test);

