declare function add5(x) {
	return x + 5;
}

declare function addWith(x, y) {
	return x + y;
}

let x as double = [1:20, 3] :> add5 :> addWith(6);

print(x);
print([1:20, 3] :> add5);
print(add5([1:20, 3]));

x <- (require "./vector.R") :> addWith(6);