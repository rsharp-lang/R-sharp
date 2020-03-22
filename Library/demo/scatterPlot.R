imports "plot.charts" from "R.plot";

let data as string = "K:\20200226\20200321\global_para.csv";
let dataTbl <- read.csv(data);

print(colnames(dataTbl));

# for(xlab in colnames(dataTbl)) {
	# for(ylab in colnames(dataTbl)) {
		# if (xlab != ylab) {
			# let x <- as.numeric(dataTbl[, xlab]);
			# let y <- as.numeric(dataTbl[, ylab]);
			# let main as string = `${xlab} ~ ${ylab}`;
			# let line <- serial(x,y, main, color = "black", ptSize = 8);
			# let save.png as string <- `${dirname(data)}/${basename(data)}/${main :> gsub("(\\|/)", "_", regexp =TRUE)}.png`;
			
			# print(save.png);
			
			# plot(line, 
				 # padding = "padding: 300px 125px 200px 250px;",
				 # x.lab   = xlab,
				 # y.lab   = ylab,
				 # legend.anchor = [1900.0, 1450.0],
				 # legendBgFill = "white",
				 # showLegend = FALSE,
				 # title = main
			# ) 
			# :> save.graphics(file = save.png)
			# ;
		# }
	# }
# }

for(ylab in colnames(dataTbl)) {
	let y <- as.numeric(dataTbl[, ylab]);
	let x <- 1:length(y);
	let line <- serial(x,y, ylab, color = "black", ptSize = 8);
	let save.png as string <- `${dirname(data)}/${basename(data)}1/${ylab :> gsub("(\\|/)", "_", regexp =TRUE)}.png`;
	
	print(save.png);
	
	plot(line, 
		 padding = "padding: 300px 125px 200px 250px;",
		 x.lab   = "X",
		 y.lab   = ylab,
		 legend.anchor = [1900.0, 1450.0],
		 legendBgFill = "white",
		 showLegend = FALSE,
		 title = ylab
	) 
	:> save.graphics(file = save.png)
	;
}