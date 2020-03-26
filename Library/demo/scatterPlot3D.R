imports "chart3D" from "R.plot";

require(grDevices.gr3D);

let file as string = "K:\20200226\20200321\PCA3.csv";
let matrix <- read.csv(file);

let x <- as.numeric(matrix[, "X"]);
let y <- as.numeric(matrix[, "Y"]);
let z <- as.numeric(matrix[, "Z"]);

let groups <- matrix[, "EigenValue"];
let keys <- unique(groups);

let data <- [];

for(key in keys) {
	let s <- serial3D(
		x[key == groups], 
		y[key == groups], 
		z[key == groups], 
		name = key, 
		color = "blue", 
		alpha = 200, 
		ptSize = 25
	);
		
	data <- data << s;	
}

let view = camera([0,0,0], -40, 256, [2560, 1440]); 
let png = `${dirname(file)}/${basename(file)}.PCA3D.png`;

plot(data, camera = camera)
:> save.graphics(file = png)
;
