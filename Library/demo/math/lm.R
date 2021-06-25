const a <- [1,2,3.2,4,5];
const b <- [2,4,6,8,10];
const mydata <- data.frame(a, b);

print(mydata);

const w = lm(b ~ a, mydata, weights = 1 / a);
const uw = lm(b ~ a, mydata);

print("weighted formula:");
print(w);

print("unweighted:");
print(uw);
print(as.formula(uw));

const test = data.frame(
	
	a = [0.91,2,3,4,5.3],
	x = [FALSE]
	
);

rownames(test) = ["alpha","beta","gamma","XX","YY"];

print(w :> predict(test));
print(uw :> predict(test));