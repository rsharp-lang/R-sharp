require(filter);

setwd(@dir);

let image = readImage("./hqx-demo.png");

bitmap(file = "./hqx_2x.png") {
	hqx_scales(image, "Hqx_2x");
}

bitmap(file = "./hqx_3x.png") {
	hqx_scales(image, "Hqx_3x");
}

bitmap(file = "./hqx_4x.png") {
	hqx_scales(image, "Hqx_4x");
}

image = readImage("./pixel.bmp");

bitmap(file = "./pixel-hqx_2x.png") {
	hqx_scales(image, "Hqx_2x");
}

bitmap(file = "./pixel-hqx_3x.png") {
	hqx_scales(image, "Hqx_3x");
}

bitmap(file = "./pixel-hqx_4x.png") {
	hqx_scales(image, "Hqx_4x");
}

image = readImage("./cat.bmp");

bitmap(file = "./cat-hqx_2x.png") {
	hqx_scales(image, "Hqx_2x");
}

bitmap(file = "./cat-hqx_3x.png") {
	hqx_scales(image, "Hqx_3x");
}

bitmap(file = "./cat-hqx_4x.png") {
	hqx_scales(image, "Hqx_4x");
}

image = readImage("./tumblr_lqb1p7EIoF1qcsurn.png");

bitmap(file = "./avatar-hqx_2x.png") {
	hqx_scales(image, "Hqx_2x");
}

bitmap(file = "./avatar-hqx_3x.png") {
	hqx_scales(image, "Hqx_3x");
}

bitmap(file = "./avatar-hqx_4x.png") {
	hqx_scales(image, "Hqx_4x");
}