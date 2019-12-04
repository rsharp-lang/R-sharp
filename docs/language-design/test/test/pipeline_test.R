let x as double;

declare function add5(x) {
	return x + 5;
}

[1:100,2]
| add5               # apply a function
| x => (x ^ 2) / 99  # apply a lambda function
| mean               # apply a function
:> x;                # assign the result value into variable x