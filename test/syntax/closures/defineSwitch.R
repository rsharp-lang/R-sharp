let type = "mean";
let x = runif(5,10);

print(x);

let y =	switch(type, default -> stop(`invalid method name '${type}'!`)) {
		mean   = mean(x),
		median = median(x),
		sum    = sum(x),
		max    = max(x),
		min    = min(x),
		sqrt   = sqrt(x),
		err    = {
			stop("this is an error test in switch expression!");
		}
	}

	print(y);