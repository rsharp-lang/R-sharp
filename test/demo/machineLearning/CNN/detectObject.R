imports "CNN" from "MLkit";

options(strict = FALSE);

convolutionalNeuralNetwork = CeNiN("P:\imagenet-matconvnet-vgg-f_new.cenin");

sink(file = `${@dir}/run_cnn.txt`);

print("loaded CNN model:");
print(toString(convolutionalNeuralNetwork));

for(imgfile in list.files(@dir, pattern = "*.jpg")) {
	cat("\n\n\n");
	print(basename(imgfile));

	result = convolutionalNeuralNetwork
	|> detectObject(target = readImage(file = imgfile))
	;
	
	result |> head(n = 10) |> print();
	result |> write.csv(
		file      = `${@dir}/${basename(imgfile)}.csv`, 
		row.names = FALSE
	)
	;
	
	cat("\n\n\n");
}

sink();

