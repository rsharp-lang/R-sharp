imports "filter" from "graphics";

setwd(@dir);

bitmap(file = "") {
	readImage("lena.jpg") |> emboss;
}