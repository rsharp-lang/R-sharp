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
	let save.png as string <- `${dirname(data)}/${basename(data)}1/${ylab :> gsub("(\\|/)", "_", regexp =TRUE)}.png`;
	
	print(save.png);
	
	volinPlot(y, 
		 margin = "padding: 300px 150px 200px 300px;",
		 size   = "2700,2700",
		 x.lab   = "X",
		 ylab   = ylab,
		 legend.anchor = [1900.0, 1450.0],
		 legendBgFill = "white",
		 showLegend = FALSE,
		 title = `volin plot of ${ylab}`,
		 labelAngle = 0,
		 colorSet = "#94cac1"
	) 
	:> save.graphics(file = save.png)
	;
}