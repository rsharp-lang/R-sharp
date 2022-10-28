require(filter);

setwd(@dir);

const image = readImage("./s042_229_continuous_large.png");

bitmap(file = "./hqx_2x.png") {
	hqx_scales(image, "Hqx_2x");
}

bitmap(file = "./hqx_3x.png") {
	hqx_scales(image, "Hqx_3x");
}

bitmap(file = "./hqx_4x.png") {
	hqx_scales(image, "Hqx_4x");
}