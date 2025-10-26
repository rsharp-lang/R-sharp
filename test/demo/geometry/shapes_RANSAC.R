imports "geometry2D" from "graphics";
imports "machineVision" from "signalKit";

let multishapes = read.csv(system.file("data/multishapes.csv", package = "REnv"), row.names = NULL, check.names = FALSE);
let t0 = new geo_transform(
	theta = 1,
	tx = 0.3,
	ty = -0.5,
	scalex = 0.995,
	scaley = 0.992
);

print(multishapes, max.print = 16);
print("test geometry transform parameters:");
print(t0);

multishapes = polygon2D(multishapes$x,multishapes$y);

let test = geo_transform(multishapes, transform = t0);

# print(multishapes);
# print(test);

let t = RANSAC(test, multishapes);

print("calculated geometry transform parameter by RANSAC method:");
print(t);

let aligned = as.data.frame( geo_transform(test, t));

test = as.data.frame(test);
test[,"class"] = "test";

multishapes = as.data.frame(multishapes );
multishapes[,"class"]="original";

aligned[,"class"] = "aligned";
aligned = rbind( rbind(multishapes, aligned), test);

bitmap(file = relative_work("RANSAC_aligned.png")) {
    plot(as.numeric(aligned$x),as.numeric(aligned$y), class = aligned$class, fill = "white",point_size= 4, colors = "paper");
}