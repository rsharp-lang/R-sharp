imports "Matrix" from "Rlapack";

A = data.frame(a1 = [2,2], a2 = [2,2]);
rownames(A) = ["a","b"];

print(A);

print(eigen(A));