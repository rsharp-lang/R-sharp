require(grDevices3D);
require(charts);

const sigma = 10;
const rho   = 28;
const beta  = 8 / 3;

const lorenz = [
	x -> sigma * (y - x),
    y -> x * (rho - z) - y,
    z -> x * y - beta * z
]
|> deSolve(
	list(x = 1,y = 1, z = 1), 
	a = 0, 
	b = 40
)
;

lorenz
|> as.data.frame 
|> write.csv(file = `${dirname(@script)}/lorenz.csv`)
;

bitmap(file = `${dirname(@script)}/lorenz.png`) {
	const view = camera(viewAngle = [30, 30, 30]);

	lorenz
	|> plot(
		vector = list(x = "x", y = "y", z = "z" ), 
		camera = view,
		color  = "brown"
	)
	;
}