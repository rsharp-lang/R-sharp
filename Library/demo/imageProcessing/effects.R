imports "filter" from "graphics";

setwd(@dir);

bitmap(file = "./lena_emboss.png") {
	readImage("lena.jpg") |> emboss;
}

bitmap(file = "./lena_pencil.png") {
	readImage("lena.jpg") |> pencil;
}

bitmap(file = "./lena_wood_carving.png") {
	readImage("lena.jpg") |> wood_carving;
}

bitmap(file = "./lena_diffusion.png") {
	readImage("lena.jpg") |> diffusion;
}

bitmap(file = "./lena_soft.png") {
	readImage("lena.jpg") |> soft;
}

bitmap(file = "./lena_sharp.png") {
	readImage("lena.jpg") |> sharp;
}