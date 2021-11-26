imports "CNN" from "MLkit";

const convolutionalNeuralNetwork = CeNiN("P:\imagenet-matconvnet-vgg-f.cenin");

for(imgfile in list.files(@dir, pattern = "*.jpg")) {
	convolutionalNeuralNetwork
	|> detectObject(target = readImage(file = imgfile))
	|> [
		head(n = 10) |> print(),
		write.csv(
			file = `${@dir}/${basename(imgfile)}.csv`, 
			row.names = FALSE
		)
	]
	;
}