let runFormulaFit as function(a = [1,2,3.2,4,5], b = [2,4,6,8,10]) {
	let mydata <- data.frame(a, b);

	print(mydata);

	let w = lm(b ~ a, mydata, weights = 1 / a);
	let uw = lm(b ~ a, mydata);

	print("weighted formula:");
	print(w);

	print("unweighted:");
	print(uw);

	# print(as.formula(uw));

	let test = data.frame(
		
		a = [0.91,2,3,4,5.3],
		x = [FALSE]
		
	);

	rownames(test) = ["alpha","beta","gamma","XX","YY"];

	print(w :> predict(test));
	print(uw :> predict(test));
}

