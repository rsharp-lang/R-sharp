require(Matrix);

let A <- matrix(c(2, 3, 5, 4, 6, 7, 8, 9, 10), nrow = 3, byrow = TRUE);
let [W, H, cost] = nmf(A, rank = 2);

print(as.data.frame(A));
print(as.data.frame(dot(W, H)));
print(as.data.frame(A - dot(W, H)));
print(cost);

print(as.data.frame(W));
print(as.data.frame(H));
