let a = as.set([1 2 3 4 5 6], mode = "integer");
let b = as.set([1 2 3 4 5 6], mode = "integer");

print(a == b);
print(a - b);

print(length(a - b));
print(length(a));

let c = as.set([3 4 5 6 7 8 9 0], mode = "integer");

print(a == c);
print(c == c);
print(a - c);
print(a + c);
print(a & c);

let jaccard1 = (a & b) / (a + b);
let jaccard2 = (a & c) / (a + c);

print([jaccard1 jaccard2]);

print(jaccard(as.integer(a), as.integer(b)));
print(jaccard(as.integer(a), as.integer(c)));