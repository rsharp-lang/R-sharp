imports "plot.charts" from "R.plot";

let data as string = "K:\20200226\20200321\global_para.csv";
let dataTbl <- read.csv(data);

print(colnames(dataTbl));

for(xlab in colnames(dataTbl)) {
	for(ylab in colnames(dataTbl)) {
		if (xlab != ylab) {
			let x <- as.numeric(dataTbl[, xlab]);
			let y <- as.numeric(dataTbl[, ylab]);
			let main as string = `${xlab} ~ ${ylab}`;
			let line <- serial(x,y, main, color = "blue");
			let save.png as string <- `${dirname(data)}/${basename(data)}/${main :> replace($"(\\|/)", "_")}.png`;
			
			print(save.png);
			
			plot(line) :> save.graphics(file = save.png);
		}
	}
}
