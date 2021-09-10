imports "filter" from "graphics";

setwd(@dir);

bitmap(file = "./lena_emboss.png") {
	readImage("lena.jpg") |> emboss;
}