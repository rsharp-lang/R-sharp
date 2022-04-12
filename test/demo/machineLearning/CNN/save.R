imports "CNN" from "MLkit";

using file as file("P:\imagenet-matconvnet-vgg-f_new.cenin") {
	"P:\imagenet-matconvnet-vgg-f.cenin"
	|> CeNiN() 
	|> saveModel(file)
	;
}

