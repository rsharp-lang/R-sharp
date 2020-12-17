require(base.math);
require(grDevices.gr3D);
require(plot.charts);

let lambda_test as function(dir = !script$dir) {
	setwd(!script$dir);

	let sigma = 10;
	let rho   = 28;
	let beta  = 8 / 3;

	let lorenz = [
		x -> sigma * (y - x),
		y -> x * (rho - z) - y,
		z -> x * y - beta * z
	];

	lorenz <- deSolve(lorenz, list(x = 1,y = 1, z = 1), a = 0, b = 10 );

	lorenz
	:> as.data.frame 
	:> write.csv(file = `${dir}/lorenz.csv`);

	let view = camera(viewAngle = [30,30,30]);

	lorenz
	:> plot(vector = list(x = "x", y = "y", z = "z" ), camera = view)
	:> save.graphics(file = `${dir}/lorenz.png`);
}

