imports "CNN" from "MLkit";

const convolutionalNeuralNetwork = CeNiN("P:\imagenet-matconvnet-vgg-f.cenin");

sink(file = `${@dir}/run_cnn.txt`);

for(imgfile in list.files(@dir, pattern = "*.jpg")) {
	cat("\n\n\n");
	print(basename(imgfile));

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
	
	cat("\n\n\n");
}

sink();